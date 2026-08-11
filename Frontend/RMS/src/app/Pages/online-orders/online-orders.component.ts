import { Component, OnDestroy, computed, effect, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OnlineOrdersService } from '../../Services/online-orders.service';
import { OrderQueueDto, OrderSignalRService } from '../../Services/order-signalr';
import { ToastService } from '../../Services/toast.service';
import { RiderService } from '../../Services/rider.service';
import { Rider } from '../../Models/rider.model';

interface RiderAssignmentDraft {
  riderId: number | null;
}

type ServiceTab = 'all' | 'delivery' | 'pickup';
type StatusFilter = 'all' | 'pending' | 'riderAssigned';
type SortOrder = 'newest' | 'oldest';
type ViewMode = 'pending' | 'inProgress';
type DeliveryStatusValue = 'Approved' | 'Preparing' | 'Enroute' | 'Delivered' | 'Rejected';

@Component({
  selector: 'app-online-orders',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './online-orders.component.html',
  styleUrls: ['./online-orders.component.css']
})
export class OnlineOrdersComponent implements OnDestroy {

  pendingOrders = signal<OrderQueueDto[]>([]);
  activeOrders = signal<OrderQueueDto[]>([]);
  loading = signal<boolean>(false);
  loadingActive = signal<boolean>(false);
  busyIds = signal<Set<number>>(new Set());
  riders = signal<Rider[]>([]);
  editingRiderIds = signal<Set<number>>(new Set());

  viewMode = signal<ViewMode>('pending');
  searchTerm = signal('');
  activeTab = signal<ServiceTab>('all');
  statusFilter = signal<StatusFilter>('all');
  sortOrder = signal<SortOrder>('oldest'); // FIFO — oldest orders shown first by default
  openMenuId = signal<number | null>(null);

  /** Ticks every 30s purely so "time ago" / wait-time labels stay live without a manual refresh. */
  private nowTick = signal(Date.now());
  private tickHandle?: ReturnType<typeof setInterval>;

  private seenIds = new Set<number>();
  private assignDrafts = new Map<number, RiderAssignmentDraft>();

  allCount = computed(() => this.pendingOrders().length);
  deliveryCount = computed(() => this.pendingOrders().filter(o => this.isDelivery(o)).length);
  pickupCount = computed(() => this.pendingOrders().filter(o => !this.isDelivery(o)).length);

  // "Ready" = nothing left blocking approval: pickup/dine-in orders always qualify,
  // delivery orders need a rider allotted first.
  readyToProcessCount = computed(() =>
    this.pendingOrders().filter(o => !this.isDelivery(o) || !!o.riderId).length
  );

  todaysOnlineSales = computed(() => {
    const todayKey = new Date().toDateString();
    return this.pendingOrders()
      .filter(o => new Date(o.createdAt).toDateString() === todayKey)
      .reduce((sum, o) => sum + (o.totalAmount || 0), 0);
  });

  visibleOrders = computed(() => {
    this.nowTick();
    const term = this.searchTerm().trim().toLowerCase();
    const tab = this.activeTab();
    const status = this.statusFilter();
    const sort = this.sortOrder();

    let list = this.pendingOrders().filter(o => {
      if (tab === 'delivery' && !this.isDelivery(o)) return false;
      if (tab === 'pickup' && this.isDelivery(o)) return false;

      if (status === 'pending' && this.isDelivery(o) && !!o.riderId) return false;
      if (status === 'riderAssigned' && !(this.isDelivery(o) && !!o.riderId)) return false;

      if (term) {
        const haystack = `${o.id} ${o.customerName ?? ''} ${o.phoneNumber ?? ''}`.toLowerCase();
        if (!haystack.includes(term)) return false;
      }

      return true;
    });

    list = [...list].sort((a, b) => {
      const diff = new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
      return sort === 'newest' ? -diff : diff;
    });

    return list;
  });

  constructor(
    private onlineOrdersService: OnlineOrdersService,
    private orderSignalR: OrderSignalRService,
    private riderService: RiderService,
    private toast: ToastService
  ) {
    this.loadPending();
    this.loadActive();
    this.loadRiders();
    this.orderSignalR.startConnection();
    this.tickHandle = setInterval(() => this.nowTick.set(Date.now()), 30000);

    // 🔄 Live: new online orders arriving
    effect(() => {
      const live = this.orderSignalR.newOnlineOrders();
      if (!live) return;

      const freshOnes = live.filter(o => !this.seenIds.has(o.id));

      if (freshOnes.length > 0) {
        freshOnes.forEach(o => this.seenIds.add(o.id));
        this.playPingSound();
        this.toast.info(`New delivery order from ${freshOnes[0].customerName || 'a customer'}`);
      }

      this.pendingOrders.update(current => {
        const existingMap = new Map(current.map(o => [o.id, o]));
        live.forEach(o => existingMap.set(o.id, { ...o, expanded: existingMap.get(o.id)?.expanded ?? false }));
        return Array.from(existingMap.values());
      });
    });

    // 🔄 Live: another tab approved/rejected one of these orders
    effect(() => {
      const resolvedId = this.orderSignalR.onlineOrderResolved();
      if (resolvedId == null) return;
      this.pendingOrders.update(list => list.filter(o => o.id !== resolvedId));
    });

    // 🔄 Live: another tab assigned a rider to one of these orders
    effect(() => {
      const updated = this.orderSignalR.riderAssigned();
      if (!updated) return;
      this.pendingOrders.update(list => list.map(o =>
        o.id === updated.id ? { ...o, riderId: updated.riderId, riderName: updated.riderName } : o
      ));
    });

    // 🔄 Live: another tab (or a rider) updated an order's fulfillment status
    effect(() => {
      const updated = this.orderSignalR.deliveryStatusUpdated();
      if (!updated) return;
      if (updated.deliveryStatus === 'Delivered') {
        this.activeOrders.update(list => list.filter(o => o.id !== updated.id));
      } else {
        this.activeOrders.update(list => list.map(o => o.id === updated.id ? { ...o, deliveryStatus: updated.deliveryStatus } : o));
      }
    });
  }

  loadActive() {
    this.loadingActive.set(true);
    this.onlineOrdersService.getActive().subscribe({
      next: (orders) => {
        this.activeOrders.set(orders.map(o => ({ ...o, expanded: false })));
        this.loadingActive.set(false);
      },
      error: () => {
        this.toast.error('Failed to load in-progress online orders');
        this.loadingActive.set(false);
      }
    });
  }

  setViewMode(mode: ViewMode) {
    this.viewMode.set(mode);
  }

  // Staff can move a delivery/pickup order freely between these — not strictly sequential,
  // since real kitchens sometimes skip a stage (e.g. an instant order going straight to Enroute).
  statusOptions(order: OrderQueueDto): DeliveryStatusValue[] {
    const all: DeliveryStatusValue[] = this.isDelivery(order)
      ? ['Preparing', 'Enroute', 'Delivered']
      : ['Preparing', 'Delivered']; // pickup orders never go Enroute — no rider involved
    return all.filter(s => s !== order.deliveryStatus);
  }

  setDeliveryStatus(order: OrderQueueDto, status: DeliveryStatusValue) {
    this.setBusy(order.id, true);
    this.onlineOrdersService.updateDeliveryStatus(order.id, status as 'Preparing' | 'Enroute' | 'Delivered').subscribe({
      next: (updated) => {
        if (status === 'Delivered') {
          this.activeOrders.update(list => list.filter(o => o.id !== order.id));
        } else {
          this.activeOrders.update(list => list.map(o => o.id === order.id ? { ...o, deliveryStatus: updated.deliveryStatus } : o));
        }
        this.setBusy(order.id, false);
        this.toast.success(`Order #${order.id} marked ${status}`);
      },
      error: (err) => {
        this.setBusy(order.id, false);
        this.toast.error(err?.error?.message || `Failed to update order #${order.id}`);
      }
    });
  }

  loadRiders() {
    this.riderService.getAll().subscribe({
      next: (riders) => this.riders.set(riders.filter(r => r.isActive)),
      error: () => this.toast.error('Failed to load riders')
    });
  }

  private playPingSound() {
    try {
      const ctx = new (window.AudioContext || (window as any).webkitAudioContext)();
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();
      osc.type = 'sine';
      osc.frequency.setValueAtTime(880, ctx.currentTime);
      gain.gain.setValueAtTime(0.15, ctx.currentTime);
      gain.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.4);
      osc.connect(gain).connect(ctx.destination);
      osc.start();
      osc.stop(ctx.currentTime + 0.4);
    } catch {
      // Audio not available — non-critical, ignore
    }
  }

  loadPending() {
    this.loading.set(true);
    this.onlineOrdersService.getPending().subscribe({
      next: (orders) => {
        orders.forEach(o => this.seenIds.add(o.id));
        this.pendingOrders.set(orders.map(o => ({ ...o, expanded: false })));
        this.loading.set(false);
      },
      error: () => {
        this.toast.error('Failed to load pending online orders');
        this.loading.set(false);
      }
    });
  }

  toggleExpand(order: OrderQueueDto) {
    order.expanded = !order.expanded;
    this.pendingOrders.update(list => [...list]);
  }

  waLink(phone?: string | null): string {
    if (!phone) return '';
    const digits = phone.replace(/\D/g, '');
    return `https://wa.me/${digits}`;
  }

  serviceTypeLabel(serviceType?: string | null): string {
    switch (serviceType) {
      case 'DineIn': return 'Dine-in';
      case 'Takeaway': return 'Pickup';
      default: return 'Delivery';
    }
  }

  serviceTypeIcon(serviceType?: string | null): string {
    switch (serviceType) {
      case 'DineIn': return 'pi-table';
      case 'Takeaway': return 'pi-shopping-bag';
      default: return 'pi-car';
    }
  }

  isDelivery(order: OrderQueueDto): boolean {
    return (order.serviceType ?? 'Delivery') === 'Delivery';
  }

  timeAgo(createdAt: string): string {
    this.nowTick();
    const diffMs = Date.now() - new Date(createdAt).getTime();
    const minutes = Math.max(0, Math.floor(diffMs / 60000));
    if (minutes < 1) return 'Just now';
    if (minutes < 60) return `${minutes} min ago`;
    const hours = Math.floor(minutes / 60);
    if (hours < 24) return `${hours} hr ago`;
    return `${Math.floor(hours / 24)} d ago`;
  }

  setTab(tab: ServiceTab) {
    this.activeTab.set(tab);
  }

  setStatusFilter(status: StatusFilter) {
    this.statusFilter.set(status);
  }

  setSort(order: SortOrder) {
    this.sortOrder.set(order);
  }

  toggleMenu(orderId: number) {
    this.openMenuId.update(current => current === orderId ? null : orderId);
  }

  copyPhone(order: OrderQueueDto) {
    this.openMenuId.set(null);
    if (!order.phoneNumber) return;
    navigator.clipboard.writeText(order.phoneNumber).then(
      () => this.toast.success('Phone number copied'),
      () => this.toast.error('Could not copy phone number')
    );
  }

  openInMaps(order: OrderQueueDto) {
    this.openMenuId.set(null);
    if (!order.address) return;
    window.open(`https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(order.address)}`, '_blank', 'noopener');
  }

  private setBusy(id: number, busy: boolean) {
    this.busyIds.update(set => {
      const next = new Set(set);
      if (busy) next.add(id); else next.delete(id);
      return next;
    });
  }

  isBusy(id: number): boolean {
    return this.busyIds().has(id);
  }

  private draft(orderId: number): RiderAssignmentDraft {
    let d = this.assignDrafts.get(orderId);
    if (!d) {
      d = { riderId: null };
      this.assignDrafts.set(orderId, d);
    }
    return d;
  }

  getDraftRiderId(order: OrderQueueDto): number | null {
    return this.draft(order.id).riderId;
  }

  setDraftRiderId(order: OrderQueueDto, value: string) {
    this.draft(order.id).riderId = value ? Number(value) : null;
  }

  isEditingRider(order: OrderQueueDto): boolean {
    return !order.riderId || this.editingRiderIds().has(order.id);
  }

  assignRider(order: OrderQueueDto) {
    const d = this.draft(order.id);
    if (!d.riderId) {
      this.toast.error('Select a rider first');
      return;
    }

    this.setBusy(order.id, true);
    this.onlineOrdersService.assignRider(order.id, d.riderId).subscribe({
      next: (updated) => {
        this.pendingOrders.update(list => list.map(o =>
          o.id === order.id ? { ...o, riderId: updated.riderId, riderName: updated.riderName } : o
        ));
        this.assignDrafts.delete(order.id);
        this.editingRiderIds.update(set => {
          const next = new Set(set);
          next.delete(order.id);
          return next;
        });
        this.setBusy(order.id, false);
        this.toast.success(`Order #${order.id} assigned to ${updated.riderName}`);
      },
      error: (err) => {
        this.setBusy(order.id, false);
        this.toast.error(err?.error?.message || `Failed to assign rider to order #${order.id}`);
      }
    });
  }

  changeRider(order: OrderQueueDto) {
    this.assignDrafts.set(order.id, { riderId: order.riderId ?? null });
    this.editingRiderIds.update(set => new Set(set).add(order.id));
  }

  approve(order: OrderQueueDto) {
    this.setBusy(order.id, true);
    this.onlineOrdersService.approve(order.id).subscribe({
      next: (result) => {
        this.pendingOrders.update(list => list.filter(o => o.id !== order.id));
        this.activeOrders.update(list => [...list, { ...result.order, expanded: false }]);
        this.setBusy(order.id, false);

        if (result.counterPrinted && result.kitchenPrinted) {
          this.toast.success(`Order #${order.id} approved — both slips printed`);
        } else if (!result.counterPrinted && !result.kitchenPrinted) {
          this.toast.warn(`Order #${order.id} approved, but both prints failed — use Reprint from Order Queue`);
        } else {
          this.toast.warn(`Order #${order.id} approved, but ${result.counterPrinted ? 'kitchen' : 'counter'} print failed — use Reprint from Order Queue`);
        }
      },
      error: (err) => {
        this.setBusy(order.id, false);
        this.toast.error(err?.error?.message || `Failed to approve order #${order.id}`);
      }
    });
  }

  reject(order: OrderQueueDto) {
    if (!confirm(`Reject order #${order.id} from ${order.customerName || 'this customer'}?`)) return;

    this.setBusy(order.id, true);
    this.onlineOrdersService.reject(order.id).subscribe({
      next: () => {
        this.pendingOrders.update(list => list.filter(o => o.id !== order.id));
        this.setBusy(order.id, false);
        this.toast.warn(`Order #${order.id} rejected`);
      },
      error: (err) => {
        this.setBusy(order.id, false);
        this.toast.error(err?.error?.message || `Failed to reject order #${order.id}`);
      }
    });
  }

  ngOnDestroy() {
    this.orderSignalR.stopConnection();
    if (this.tickHandle) clearInterval(this.tickHandle);
  }
}
