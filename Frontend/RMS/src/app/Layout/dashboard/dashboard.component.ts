import { Component, OnDestroy, OnInit, effect, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, NavigationEnd } from '@angular/router';

import { DashboardService } from '../../Services/dashboard.service';
import { OrderSignalRService } from '../../Services/order-signalr';

import { Chart, registerables } from 'chart.js';
import { filter } from 'rxjs/operators';
import { forkJoin } from 'rxjs';
import { ChartModule } from 'primeng/chart';
import { DateRangeFilterComponent, DateRange } from '../../Shared/date-range-filter/date-range-filter.component';
import { LiveClockComponent } from '../../Shared/live-clock/live-clock.component';

Chart.register(...registerables);

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule,ChartModule,DateRangeFilterComponent,LiveClockComponent],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnDestroy {

  // Loading & Route
  loading = signal(false);
  isDashboard = signal(false);

  // Main summaries
  mainSummary = signal<any>(null);
  salesSummary = signal<any>(null);
  purchaseSummary = signal<any>(null);
  stockSummary = signal<any[]>([]);
  kitchenSummary = signal<any>(null);
  profitSummary = signal<any>(null);
  profitSummarytoday = signal<any>(null);
  dailySales = signal<any[]>([]);
  dailyPurchases = signal<any[]>([]);
  dailyProfit = signal<any[]>([]);
  utility = signal<any>(null);
  lowStock = signal<any[]>([]);
  orderCountSummary = signal<any>(null);
  vendorPaymentSummary = signal<any[]>([]);
  topCategories = signal<any[]>([]);
  stockUsage = signal<any>(null);
  salaryStatus = signal<any>(null);

  // Date-range filter (Today / Yesterday / This Week / This Month / This Year / Overall / Custom)
  periodSummary = signal<any>(null);
  periodLoading = signal(false);
  selectedRangeLabel = signal('Today');
  private rangeFrom = signal('');
  private rangeTo = signal('');

  // Chart instances
  chartData = signal<any>(null);
chartOptions = signal<any>(null);
dailyLineChartData = signal<any>(null);
dailyLineChartOptions = signal<any>(null);
topCategoryChartData = signal<any>(null);
topCategoryChartOptions = signal<any>(null);
stockUsageChartData = signal<any>(null);
stockUsageChartOptions = signal<any>(null);



  constructor(
    private dashboardService: DashboardService,
    private router: Router,
    private orderSignalR: OrderSignalRService
  ) {
    this.buildBarChart();
    this.buildDailySalesChart();
    this.buildCategoryChart();
    this.buildKitchenPieChart();
    // Check current route
    this.isDashboard.set(this.router.url === '/dashboard');
    if (this.isDashboard()) this.loadDashboard();

    // Listen to route changes
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: any) => {
        this.isDashboard.set(event.urlAfterRedirects === '/dashboard');
        if (this.isDashboard()) this.loadDashboard();


      });

    // 🔥 Effect to update low stock automatically when stockSummary changes
    effect(() => {
      this.lowStock.set(this.stockSummary().filter(x => x.totalQuantity < 5));
    });

    // Live-refresh while sitting on the dashboard and an order happens elsewhere
    // (e.g. another cashier taking an order) — no more needing to leave and come back.
    this.orderSignalR.startConnection();
    effect(() => {
      const activity = this.orderSignalR.lastActivityAt();
      if (activity === 0) return; // skip initial value
      if (this.isDashboard()) this.loadDashboard();
    });
  }

  ngOnDestroy(): void {
    this.orderSignalR.stopConnection();
  }

  

  loadDashboard() {
    this.loading.set(true);

    forkJoin({
      main: this.dashboardService.getMainSummary(),
      sales: this.dashboardService.getSalesSummary(),
      purchase: this.dashboardService.getPurchaseSummary(),
      stock: this.dashboardService.getStockSummary(),
      kitchen: this.dashboardService.getkitchenwastetoday(),
      profit: this.dashboardService.getProfitSummary(),
      profittoday: this.dashboardService.getProfitSummarytoday(),
      dailySales: this.dashboardService.getDailySales(),
      dailyPurchases: this.dashboardService.getDailyPurchases(),
      dailyProfit: this.dashboardService.getDailyProfits(),
      orderCount: this.dashboardService.getOrderCountSummary(),
      vendors: this.dashboardService.getVendorPaymentSummary(),
      categories: this.dashboardService.getTopSellingCategories(),
      stockUsage: this.dashboardService.getStockUsagePercentage(),
      utility: this.dashboardService.getMonthlyUtilityBills(),
      salaryStatus: this.dashboardService.getSalaryStatusSummarytoday()
    }).subscribe({
      next: (res) => {
        this.mainSummary.set(res.main);
        this.salesSummary.set(res.sales);
        this.purchaseSummary.set(res.purchase);
        this.stockSummary.set(res.stock);
        this.kitchenSummary.set(res.kitchen);
        this.profitSummary.set(res.profit);
        this.profitSummarytoday.set(res.profittoday?.length ? res.profittoday[0] : null);
        this.dailySales.set(res.dailySales);
        this.dailyPurchases.set(res.dailyPurchases);
        this.dailyProfit.set(res.dailyProfit);
        this.orderCountSummary.set(res.orderCount);
        this.vendorPaymentSummary.set(res.vendors);
        this.topCategories.set(res.categories);
        this.stockUsage.set(res.stockUsage);
        this.utility.set(res.utility);
        this.salaryStatus.set(res.salaryStatus);

        // Build charts after data is ready
        //this.buildDailySalesChart();
        
        //this.buildDailyProfitChart();
        //this.buildKitchenPieChart();
        //this.buildCategoryChart();
        // this.buildVendorPaymentChart();

        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  getMonthYearText() {
    return new Date().toLocaleString("en-US", { month: "long", year: "numeric" });
  }

  countBySource(orders: any[] | undefined | null, source: 'Online' | 'Site'): number {
    if (!orders) return 0;
    return source === 'Online'
      ? orders.filter(o => o.orderSource === 'Online').length
      : orders.filter(o => o.orderSource !== 'Online').length;
  }

  // ================= DATE RANGE FILTER =================
  onRangeChange(range: DateRange) {
    this.rangeFrom.set(range.from);
    this.rangeTo.set(range.to);

    const presetLabels: Record<string, string> = {
      today: 'Today',
      yesterday: 'Yesterday',
      thisWeek: 'This Week',
      thisMonth: 'This Month',
      thisYear: 'This Year',
      overall: 'Overall',
      custom: `${range.from} → ${range.to}`
    };
    this.selectedRangeLabel.set(presetLabels[range.preset] ?? range.preset);

    this.periodLoading.set(true);
    this.dashboardService.getReportByRange(range.from, range.to).subscribe({
      next: (res) => {
        this.periodSummary.set(res);
        this.periodLoading.set(false);
      },
      error: () => this.periodLoading.set(false)
    });

    forkJoin({
      sales: this.dashboardService.getSalesByDateRange(range.from, range.to),
      purchases: this.dashboardService.getPurchasesByDateRange(range.from, range.to)
    }).subscribe({
      next: (res) => {
        this.dailySales.set(res.sales ?? []);
        this.dailyPurchases.set(res.purchases ?? []);
      }
    });
  }



  // ================= DAILY SALES LINE CHART =================
  

  // ================= DAILY PROFIT CHART =================

  buildKitchenPieChart() {
   effect(() => {
    const usage = this.stockUsage();

    if (!usage) return;

    this.stockUsageChartData.set({
      labels: ['Kitchen Out', 'Waste'],
      datasets: [
        {
          data: [
            usage.kitchenOutPercentage,
            usage.wastePercentage
          ]
        }
      ]
    });

    this.stockUsageChartOptions.set({
      responsive: true,
      maintainAspectRatio: false, 
      cutout: '70%', // doughnut thickness
      plugins: {
        legend: {
          position: 'bottom'
        },
        tooltip: {
          callbacks: {
            label: (ctx: any) =>
              `${ctx.label}: ${ctx.raw}%`
          }
        }
      }
    });
  });
  }


  buildCategoryChart() {
    effect(() => {
    const categories = this.topCategories();

    if (!categories || !categories.length) return;

    // Sort descending (top-selling first)
    const sorted = [...categories].sort(
      (a, b) => b.totalOrders - a.totalOrders
    );

    this.topCategoryChartData.set({
      labels: sorted.map(c => c.categoryName),
      datasets: [
        {
          label: 'Total Orders',
          data: sorted.map(c => c.totalOrders),
          borderWidth: 1
        }
      ]
    });

    this.topCategoryChartOptions.set({
      indexAxis: 'y', // 🔥 Horizontal bar
      responsive: true,
      plugins: {
        legend: {
          display: false
        },
        tooltip: {
          callbacks: {
            label: (ctx: any) => `Orders: ${ctx.raw}`
          }
        }
      },
      scales: {
        x: {
          beginAtZero: true,
          title: {
            display: true,
            text: 'Number of Orders'
          }
        }
      }
    });
  });
  }
  // ================= BAR CHART =================
  buildBarChart() {
    // Replace with your real API
    effect(() => {
  const sales = this.salesSummary();
  const purchase = this.purchaseSummary();
  const profit = this.profitSummary();

  if (!sales || !purchase || !profit) return;

  this.chartData.set({
    labels: ['Today', 'Weekly', 'Monthly'],
    datasets: [
      {
        label: 'Sales',
        data: [
          sales.todayTotal,
          sales.weeklyTotal,
          sales.monthlyTotal
        ],
        backgroundColor: '#4caf50'
      },
      {
        label: 'Purchase',
        data: [
          purchase.todayTotal,
          purchase.weeklyTotal,
          purchase.monthlyTotal
        ],
        backgroundColor: '#f44336'
      },
      {
        label: 'Profit',
        data: [
          profit.todayProfit,
          profit.weeklyProfit,
          profit.monthlyProfit
        ],
        backgroundColor: '#2196f3'
      }
    ]
  });

  this.chartOptions.set({
    responsive: true,
    plugins: {
      legend: { position: 'top' }
    },
    scales: {
      x: { stacked: false },
      y: {
        stacked: false,
        beginAtZero: true,
        title: { display: true, text: 'Amount' }
      }
    }
  });
});



  }
  buildDailySalesChart() {
   effect(() => {
    const sales = this.dailySales();
    const purchases = this.dailyPurchases();
    const from = this.rangeFrom();
    const to = this.rangeTo();

    if (!sales.length && !purchases.length) return;

    // 1️⃣ Build the list of days to plot: the selected range if we have one,
    //    otherwise fall back to whatever dates actually appear in the data.
    const salesMap = new Map<string, number>();
    const purchaseMap = new Map<string, number>();

    sales.forEach(s =>
      salesMap.set(s.date.substring(0, 10), s.totalAmount)
    );

    purchases.forEach(p =>
      purchaseMap.set(p.date.substring(0, 10), p.totalAmount)
    );

    const dayKeys: string[] = [];

    if (from && to) {
      const cursor = new Date(from + 'T00:00:00');
      const last = new Date(to + 'T00:00:00');

      while (cursor <= last) {
        dayKeys.push(cursor.toLocaleDateString('en-CA'));
        cursor.setDate(cursor.getDate() + 1);
      }
    } else {
      const allDates = new Set([...salesMap.keys(), ...purchaseMap.keys()]);
      dayKeys.push(...Array.from(allDates).sort());
    }

    // 2️⃣ Build aligned arrays
    const labels: string[] = [];
    const salesData: number[] = [];
    const purchaseData: number[] = [];

    for (const date of dayKeys) {
      labels.push(date.substring(5)); // MM-DD, compact enough for long ranges
      salesData.push(salesMap.get(date) ?? 0);
      purchaseData.push(purchaseMap.get(date) ?? 0);
    }

    // 3️⃣ Chart.js data
    this.dailyLineChartData.set({
      labels,
      datasets: [
        {
          label: 'Daily Sales',
          data: salesData,
          tension: 0.4,
          fill: false
        },
        {
          label: 'Daily Purchases',
          data: purchaseData,
          tension: 0.4,
          fill: false
        }
      ]
    });

    this.dailyLineChartOptions.set({
      responsive: true,
      plugins: {
        legend: { position: 'top' }
      },
      scales: {
        y: {
          beginAtZero: true
        },
        x: {
          title: {
            display: true,
            text: 'Day of Month'
          }
        }
      }
    });
  });
  }

}
