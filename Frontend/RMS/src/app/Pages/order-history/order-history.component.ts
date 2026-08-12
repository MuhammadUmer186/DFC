import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PaginatorModule } from 'primeng/paginator';
import { Order, OrderService } from '../../Services/order.service';
import { PrintService } from '../../Services/printservice';
import { ToastService } from '../../Services/toast.service';
import { RmsCurrencyPipe } from '../../Shared/pipes/currency-symbol.pipe';

@Component({
  selector: 'app-orders-history',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginatorModule, RmsCurrencyPipe],
  templateUrl: './order-history.component.html',
  styleUrls: ['./order-history.component.css']
})
export class OrdersHistoryComponent {

  orders = signal<Order[]>([]);
  loading = signal(true);

  // Pagination signals
  first = signal(0);      // index of first record on current page
  rows = signal(5);       // page size
  totalRecords = signal(0);
   printOrderId = signal<number | null>(null);
  printing = signal(false);

  constructor(private orderService: OrderService,private printService:PrintService,private toast:ToastService) {
    this.loadOrders(0, this.rows());
  }

  loadOrders(page: number, pageSize: number) {
    this.loading.set(true);
    this.orderService.getPagedOrders(page, pageSize).subscribe({
      next: (res) => {
        this.orders.set(res.items);
        this.totalRecords.set(res.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onPageChange(event: any) {
    const page = event.first / event.rows; // current page index
    this.first.set(event.first);
    this.rows.set(event.rows);
    this.loadOrders(page, event.rows);
  }
  
  printOrderById() {
  if (!this.printOrderId()) {
    alert("Please enter an Order Number");
    return;
  }

  this.printing.set(true);

  this.orderService.getOrderById(this.printOrderId()!)
    .subscribe({
      next: (order) => {

        const receiptDto = {
          copyType: 'customer' as const,
          orderNo: order.id,
          total: order.totalAmount,
          discount: 0,
          finalTotal: order.totalAmount,
          items: [
            ...order.items.map(i => ({
              name: i.menuItemName,
              quantity: i.quantity,
              price: i.unitPrice
            })),
            ...(order.deals ?? []).map(d => ({
              name: d.dealName,
              quantity: d.quantity,
              price: d.dealPrice
            }))
          ]
        };

        this.printService.printReceipt(receiptDto)
          .subscribe({
            next: () => {
              this.toast.success("Receipt printed successfully");
              this.printing.set(false);
            },
            error: err => {
              console.error(err);
              this.toast.error("Print failed");
              this.printing.set(false);
            }
          });
      },
      error: () => {
        this.toast.error("Order not found");
        this.printing.set(false);
      }
    });
}
}
