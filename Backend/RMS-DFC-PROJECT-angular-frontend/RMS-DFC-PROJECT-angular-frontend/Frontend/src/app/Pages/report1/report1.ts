import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DashboardService } from '../../Services/dashboard.service';
import { OrderService, Order } from '../../Services/order.service';
import { PurchaseOrder, PurchaseOrderService } from '../../Services/purchaseorder.service';
import { KitchenOut, KitchenOutService } from '../../Services/kitchenout.service';

@Component({
  selector: 'app-report',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './report1.html',
  styleUrls: ['./report1.css']
})
export class ReportComponent {

  date = signal('');
  loading = signal(false);

  report = signal<any>(null);

  orders = signal<Order[]>([]);
  purchaseOrders = signal<PurchaseOrder[]>([]);
  kitchenOuts = signal<KitchenOut[]>([]);

  totalRecords = signal(0);
  first = signal(0);
  rows = signal(5);

  expandedOrderId = signal<number | null>(null);
  expandedKitchenId = signal<number | null>(null);

  // 🔥 SINGLE SOURCE OF TRUTH
  selectedDate: string = '';

  constructor(
    private dashboardService: DashboardService,
    private orderService: OrderService,
    private purchaseOrderService: PurchaseOrderService,
    private kitchenOutService: KitchenOutService
  ) {}

  // ⭐ MAIN REPORT LOAD
  loadReport() {

  if (!this.date()) {
    alert("Please select date");
    return;
  }

  this.loading.set(true);

  this.selectedDate = this.date();

  this.dashboardService.getDailyReport(this.selectedDate).subscribe({

    next: (res) => {

      this.report.set(res);

      this.orders.set(res.orders ?? []);
      this.purchaseOrders.set(res.purchaseOrders ?? []);
      this.kitchenOuts.set(res.kitchenOuts ?? []);

      this.loading.set(false);
    },

    error: () => {
      this.loading.set(false);
    }

  });

}

  // ⭐ LOAD ORDERS
  loadOrders(page: number, pageSize: number, date: string) {
    this.orderService.getPagedOrdersByDate(date, page, pageSize).subscribe({
      next: (res) => {
        this.orders.set(res.items ?? []);
        this.totalRecords.set(res.totalCount);
      }
    });
  }

  toggleExpand(order: any) {
    order.expanded = !order.expanded;
    this.orders.set([...this.orders()]);
  }

  // ⭐ PURCHASE ORDERS
  loadPurchaseOrders(date: string) {
    this.purchaseOrderService.getByDate(date).subscribe({
      next: (res) => this.purchaseOrders.set(res ?? [])
    });
  }

  // ⭐ KITCHEN OUTS
  loadKitchenOuts(date: string) {
    this.kitchenOutService.getPagedByDate(date, 0, 50).subscribe({
      next: (res) => this.kitchenOuts.set(res.items ?? [])
    });
  }

  toggleDetails(id: number) {
    this.expandedOrderId.set(this.expandedOrderId() === id ? null : id);
  }

  toggleKitchenDetails(id: number) {
    this.expandedKitchenId.set(this.expandedKitchenId() === id ? null : id);
  }

  // ⭐ PAGINATION FIXED
  onPageChange(event: any) {
    this.first.set(event.first);
    this.rows.set(event.rows);

    const page = event.first / event.rows;

    this.loadOrders(page, event.rows, this.selectedDate);
  }

  // ⭐ DELETE FIXED
  deleteOrder(id: number) {
    if (!confirm("Delete this order?")) return;

    this.orderService.DeleteOrderById(id).subscribe(() => {
      const page = this.first() / this.rows();
      this.loadOrders(page, this.rows(), this.selectedDate);
    });
  }
}