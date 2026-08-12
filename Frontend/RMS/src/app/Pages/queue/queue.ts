import { Component, OnDestroy, effect, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { QueueService } from '../../Services/queue';
import { PrintService, PrinterType } from '../../Services/printservice';
import { ToastService } from '../../Services/toast.service';
import { OrderQueueDto, OrderSignalRService } from '../../Services/order-signalr';
import { OnlineOrdersService } from '../../Services/online-orders.service';
import { nextDeliveryStatus, DeliveryStatusValue } from '../../Services/delivery-status.util';

@Component({
  selector: 'app-queue',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './queue.html',
  styleUrls: ['./queue.css']
})
export class QueueComponent implements OnDestroy {

  queuedOrders = signal<OrderQueueDto[]>([]);
  loading = signal<boolean>(false);
  paymentMethod = signal<string>('Cash');
  busyIds = signal<Set<number>>(new Set());

  customerPrinter = signal<PrinterType>('usb1');
  kitchenPrinter = signal<PrinterType>('usb2');

  constructor(
    private queueService: QueueService,
    private printService: PrintService,
    private toast: ToastService,
    private orderSignalR: OrderSignalRService,
    private onlineOrdersService: OnlineOrdersService
  ) {
    this.loadQueuedOrders();
    this.orderSignalR.startConnection();

    // 🔄 Live: delivery-status changed elsewhere (Online Orders page, or a rider's app)
    effect(() => {
      const updated = this.orderSignalR.deliveryStatusUpdated();
      if (!updated) return;
      this.queuedOrders.update(list => list.map(o =>
        o.id === updated.id ? { ...o, deliveryStatus: updated.deliveryStatus, riderName: updated.riderName, paid: updated.paid } : o
      ).filter(o => !o.paid));
    });

    // 🔄 Live SignalR sync
    effect(() => {
  const liveOrders = this.orderSignalR.queuedOrders();
  if (!liveOrders) return;

  this.queuedOrders.update(current => {
    const existingMap = new Map(current.map(o => [o.id, o]));

    liveOrders
      .filter(o => !o.paid) // ✅ KEY LINE
      .forEach(o => {
        existingMap.set(o.id, {
          ...o,
          discount: o.discount ?? 0,
          expanded: existingMap.get(o.id)?.expanded ?? false,
          takenByEmployeeName: o.takenByEmployeeName || 'Unknown'
        });
      });

    return Array.from(existingMap.values());
  });
});

  }

  loadQueuedOrders() {
    this.loading.set(true);

    this.queueService.getQueuedOrders().subscribe({
      next: (orders) => {
        this.queuedOrders.set(
          orders.map(o => ({
            ...o,
            discount: o.discount ?? 0,            // ✅ FIX
            expanded: false,
            takenByEmployeeName: o.takenByEmployeeName || 'Unknown'
          }))
        );
        this.loading.set(false);
      },
      error: () => {
        this.toast.error('Failed to load queued orders!');
        this.loading.set(false);
      }
    });
  }

  toggleExpand(order: OrderQueueDto) {
    order.expanded = !order.expanded;
    this.queuedOrders.update(list => [...list]);
  }

  serviceTypeLabel(serviceType?: string | null): string {
    switch (serviceType) {
      case 'DineIn': return '🍽️ Dine-in';
      case 'Takeaway': return '🥡 Takeaway';
      default: return '🛵 Delivery';
    }
  }

  confirmPay(order: OrderQueueDto) {
    const cashierId = Number(localStorage.getItem('employeeId'));

    this.queueService.payOrder(order.id, this.paymentMethod(), cashierId).subscribe({
      next: (updatedOrder: any) => {

        // ✅ Build DTO safely
        const dto: OrderQueueDto = {
          id: updatedOrder.id,
          totalAmount: updatedOrder.totalAmount,
          createdAt: updatedOrder.createdAt,
          paid: updatedOrder.paid,
          discount: order.discount ?? 0,           // ✅ FIX
          takenByEmployeeName: updatedOrder.takenByEmployeeName || 'Unknown',
          items: updatedOrder.items ?? [],
          deals: updatedOrder.deals ?? [],
          expanded: false
        };

        // 🧾 Receipt items
        const receiptItems = [
          // Menu items
          ...dto.items.map(i => ({
            name: i.menuItemName,
            quantity: i.quantity,
            price: i.unitPrice
          })),

          // Deals as single line
          ...dto.deals.map(d => ({
            name: d.dealName,
            quantity: d.quantity,
            price: d.dealPrice
          }))
        ];

        // 🖨 Customer receipt
        // 🖨 Customer receipt
this.printService.printReceipt({
  copyType: 'customer',
  orderNo: dto.id,
  total: dto.totalAmount + (dto.discount ?? 0), // ❌ was just "discount"
  discount: dto.discount ?? 0,                  // ❌ was dto.discount
  finalTotal: dto.totalAmount,
  items: receiptItems
}, this.customerPrinter()).subscribe();

// 🖨 Kitchen receipt
// this.printService.printReceipt({
//   copyType: 'kitchen',
//   orderNo: dto.id,
//   total: dto.totalAmount + (dto.discount ?? 0),
//   discount: dto.discount ?? 0,
//   finalTotal: dto.totalAmount,
//   items: receiptItems
// }, this.kitchenPrinter()).subscribe();


        // ✅ Remove from queue
        this.queuedOrders.update(list =>
          list.filter(o => o.id !== order.id)
        );

        this.toast.success(`Order #${order.orderNumber || order.id} marked as paid`);
      },
      error: () => this.toast.error(`Failed to pay Order #${order.orderNumber || order.id}`)
    });
  }

  cancelOrder(order: OrderQueueDto) {
  const cashierId = Number(localStorage.getItem('employeeId'));

  this.queueService.cancelOrder(order.id, cashierId).subscribe({
    next: () => {

      // ✅ Print kitchen notice for cancellation
      const cancelReceipt = {
        copyType: 'kitchen' as const,
        orderNo: order.id,
        total: 0,                // total doesn't matter for cancelled notice
        discount: 0,
        finalTotal: 0,
        items: [
          {
            name: `ORDER #${order.orderNumber || order.id} CANCELLED`,
            quantity: 1,
            price: 0
          }
        ]
      };

      // this.printService.printReceipt(cancelReceipt, this.kitchenPrinter()).subscribe({
      //   next: () => {
      //     this.toast.info(`Kitchen notified about cancellation of Order #${order.id}`);
      //   },
      //   error: () => {
      //     this.toast.error(`Failed to print cancellation for Order #${order.id}`);
      //   }
      // });

      // ✅ Remove from queue
      this.queuedOrders.update(list =>
        list.filter(o => o.id !== order.id)
      );

      this.toast.warn(`Order #${order.orderNumber || order.id} canceled`);
    },
    error: () => this.toast.error(`Failed to cancel Order #${order.orderNumber || order.id}`)
  });
}

  isBusy(id: number): boolean {
    return this.busyIds().has(id);
  }

  private setBusy(id: number, busy: boolean) {
    this.busyIds.update(set => {
      const next = new Set(set);
      if (busy) next.add(id); else next.delete(id);
      return next;
    });
  }

  // Same one-step-at-a-time fulfillment flow as the Online Orders "In Progress" tab.
  nextStatus(order: OrderQueueDto): DeliveryStatusValue | null {
    return nextDeliveryStatus(order);
  }

  setDeliveryStatus(order: OrderQueueDto, status: DeliveryStatusValue) {
    this.setBusy(order.id, true);
    this.onlineOrdersService.updateDeliveryStatus(order.id, status as 'Preparing' | 'Enroute' | 'Delivered').subscribe({
      next: (updated) => {
        // Order stays in the Queue (still unpaid) until payment is confirmed separately.
        this.queuedOrders.update(list => list.map(o => o.id === order.id ? { ...o, deliveryStatus: updated.deliveryStatus } : o));
        this.setBusy(order.id, false);
        this.toast.success(`Order #${order.orderNumber || order.id} marked ${status}`);
      },
      error: (err) => {
        this.setBusy(order.id, false);
        this.toast.error(err?.error?.message || `Failed to update order #${order.orderNumber || order.id}`);
      }
    });
  }

  confirmPayment(order: OrderQueueDto) {
    this.setBusy(order.id, true);
    this.onlineOrdersService.confirmPayment(order.id).subscribe({
      next: () => {
        this.queuedOrders.update(list => list.filter(o => o.id !== order.id));
        this.setBusy(order.id, false);
        this.toast.success(`Payment received for order #${order.id}`);
      },
      error: (err) => {
        this.setBusy(order.id, false);
        this.toast.error(err?.error?.message || `Failed to confirm payment for order #${order.id}`);
      }
    });
  }

  ngOnDestroy() {
    this.orderSignalR.stopConnection();
  }
}
