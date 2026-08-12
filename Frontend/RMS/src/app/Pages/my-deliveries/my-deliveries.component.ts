import { Component, OnInit, signal } from '@angular/core';
import { MyDeliveriesService } from '../../Services/my-deliveries.service';
import { AuthService } from '../../Services/auth.service';
import { ToastService } from '../../Services/toast.service';
import { OrderQueueDto } from '../../Services/order-signalr';
import { RmsCurrencyPipe } from '../../Shared/pipes/currency-symbol.pipe';

@Component({
  selector: 'app-my-deliveries',
  standalone: true,
  imports: [RmsCurrencyPipe],
  templateUrl: './my-deliveries.component.html',
  styleUrl: './my-deliveries.component.css'
})
export class MyDeliveriesComponent implements OnInit {
  orders = signal<OrderQueueDto[]>([]);
  loading = signal(false);
  busyIds = signal<Set<number>>(new Set());

  constructor(
    private service: MyDeliveriesService,
    public auth: AuthService,
    private toast: ToastService
  ) {}

  ngOnInit() {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.service.getMyDeliveries().subscribe({
      next: (orders) => {
        this.orders.set(orders);
        this.loading.set(false);
      },
      error: () => {
        this.toast.error('Failed to load your deliveries');
        this.loading.set(false);
      }
    });
  }

  waLink(phone?: string | null): string {
    if (!phone) return '';
    const digits = phone.replace(/\D/g, '');
    return `https://wa.me/${digits}`;
  }

  mapsLink(order: OrderQueueDto): string {
    if (order.latitude == null || order.longitude == null) return '';
    return `https://www.google.com/maps/search/?api=1&query=${order.latitude},${order.longitude}`;
  }

  isBusy(id: number): boolean {
    return this.busyIds().has(id);
  }

  markDelivered(order: OrderQueueDto) {
    this.busyIds.update(set => new Set(set).add(order.id));
    this.service.markDelivered(order.id).subscribe({
      next: () => {
        this.orders.update(list => list.map(o => o.id === order.id ? { ...o, deliveryStatus: 'Delivered' } : o));
        this.busyIds.update(set => { const next = new Set(set); next.delete(order.id); return next; });
        this.toast.success(`Order #${order.orderNumber || order.id} marked delivered`);
      },
      error: (err) => {
        this.busyIds.update(set => { const next = new Set(set); next.delete(order.id); return next; });
        this.toast.error(err?.error?.message || `Failed to update order #${order.orderNumber || order.id}`);
      }
    });
  }

  confirmPayment(order: OrderQueueDto) {
    this.busyIds.update(set => new Set(set).add(order.id));
    this.service.confirmPayment(order.id).subscribe({
      next: () => {
        this.orders.update(list => list.map(o => o.id === order.id ? { ...o, paid: true } : o));
        this.busyIds.update(set => { const next = new Set(set); next.delete(order.id); return next; });
        this.toast.success(`Payment received for order #${order.orderNumber || order.id}`);
      },
      error: (err) => {
        this.busyIds.update(set => { const next = new Set(set); next.delete(order.id); return next; });
        this.toast.error(err?.error?.message || `Failed to confirm payment for order #${order.orderNumber || order.id}`);
      }
    });
  }
}
