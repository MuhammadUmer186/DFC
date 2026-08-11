import { Component, Input, OnDestroy, OnInit, effect, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AuthService } from '../../../../Services/auth.service';
import { DashboardService } from '../../../../Services/dashboard.service';
import { OrderSignalRService } from '../../../../Services/order-signalr';
import { BrandingService } from '../../../../Services/branding.service';

interface TodaySummary {
  orders: number;
  sales: number;
  avgOrderValue: number;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css']
})
export class SidebarComponent implements OnInit, OnDestroy {
  @Input() closed = false;

  role: string | null = null;
  currentYear = new Date().getFullYear();

  /** Currently open dropdown menu */
  openMenu: string | null = null;

  todaySummary = signal<TodaySummary | null>(null);
  private summaryConnectionStarted = false;

  constructor(
    private auth: AuthService,
    private dashboardService: DashboardService,
    private orderSignalR: OrderSignalRService,
    public branding: BrandingService
  ) {
    this.branding.load();
    // The sidebar stays mounted for the whole session (it's outside the router-outlet),
    // so without this it would load Today's Summary once at login and never again —
    // every order taken/paid/approved anywhere in the app would look stale until a hard
    // refresh. Re-load it whenever ANY order activity happens, anywhere.
    effect(() => {
      const activity = this.orderSignalR.lastActivityAt();
      if (activity === 0) return; // skip the initial signal value — already loaded via ngOnInit
      this.loadTodaySummary();
    });
  }

  ngOnInit(): void {
    this.role = this.auth.getRole();

    if (this.role === 'Admin' || this.role === 'MainAdmin' || this.role === 'SuperAdmin') {
      this.loadTodaySummary();
      this.orderSignalR.startConnection();
      this.summaryConnectionStarted = true;
    }
  }

  ngOnDestroy(): void {
    if (this.summaryConnectionStarted) {
      this.orderSignalR.stopConnection();
    }
  }

  private loadTodaySummary() {
    forkJoin({
      main: this.dashboardService.getMainSummary(),
      orderCount: this.dashboardService.getOrderCountSummary()
    }).subscribe({
      next: ({ main, orderCount }) => {
        const orders = orderCount?.todayOrders ?? 0;
        const sales = main?.todaySales ?? 0;

        this.todaySummary.set({
          orders,
          sales,
          avgOrderValue: orders > 0 ? sales / orders : 0
        });
      },
      error: () => this.todaySummary.set(null)
    });
  }

  /**
   * Toggle sidebar dropdown menu
   * @param menu The menu to toggle
   */
  toggleMenu(menu: string) {
    // If clicked menu is already open, close it. Otherwise, open the new menu.
    this.openMenu = this.openMenu === menu ? null : menu;
  }
}
