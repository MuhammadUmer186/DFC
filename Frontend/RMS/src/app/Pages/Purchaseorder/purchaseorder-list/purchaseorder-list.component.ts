import { Component, signal, effect } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { PurchaseOrder, PurchaseOrderService } from '../../../Services/purchaseorder.service';
import { RmsCurrencyPipe } from '../../../Shared/pipes/currency-symbol.pipe';

@Component({
  standalone: true,
  selector: 'app-purchase-order-list',
  imports: [CommonModule, DatePipe, RmsCurrencyPipe],
  templateUrl: './purchaseorder-list.component.html',
  styleUrls: ['./purchaseorder-list.component.css']
})
export class PurchaseOrderListComponent {

  // Signals for reactive state
  orders = signal<PurchaseOrder[]>([]);
  loading = signal(true);
  expandedOrderId = signal<number | null>(null);

  constructor(private poService: PurchaseOrderService) {
    this.loadOrders();
  }

  // Load orders from API
  loadOrders() {
    this.loading.set(true);
    this.poService.getAll().subscribe({
      next: res => {
        this.orders.set(res);
        this.loading.set(false);
      },
      error: err => {
        console.error(err);
        this.loading.set(false);
      }
    });
  }

  // Toggle expanded order details
  toggleDetails(id: number) {
    this.expandedOrderId.set(this.expandedOrderId() === id ? null : id);
  }
}
