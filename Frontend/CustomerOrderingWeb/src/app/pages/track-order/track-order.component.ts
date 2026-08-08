import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OrderService, OrderStatusResponse } from '../../services/order.service';

@Component({
  selector: 'app-track-order',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './track-order.component.html',
  styleUrl: './track-order.component.css'
})
export class TrackOrderComponent {
  orderId = signal<number | null>(null);
  phone = signal('');

  loading = signal(false);
  errorMsg = signal<string | null>(null);
  result = signal<OrderStatusResponse | null>(null);

  constructor(private orderService: OrderService) {}

  get canSubmit(): boolean {
    return !!this.orderId() && this.phone().trim().length >= 7 && !this.loading();
  }

  track() {
    if (!this.canSubmit) return;

    this.loading.set(true);
    this.errorMsg.set(null);
    this.result.set(null);

    this.orderService.getOrderStatus(this.orderId()!, this.phone().trim()).subscribe({
      next: (res) => {
        this.result.set(res);
        this.loading.set(false);
      },
      error: (err) => {
        this.errorMsg.set(err?.error || 'Order not found. Double-check the order number and phone number.');
        this.loading.set(false);
      }
    });
  }

  serviceTypeLabel(serviceType?: string): string {
    switch (serviceType) {
      case 'DineIn': return '🍽️ Dine-in';
      case 'Takeaway': return '🥡 Takeaway';
      default: return '🛵 Delivery';
    }
  }
}
