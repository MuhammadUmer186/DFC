import { Component, OnInit, effect, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, NavigationEnd } from '@angular/router';

import { DashboardService } from '../../Services/dashboard.service';

import { Chart, registerables } from 'chart.js';
import { filter } from 'rxjs/operators';
import { forkJoin } from 'rxjs';
import { ChartModule } from 'primeng/chart';

Chart.register(...registerables);

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule,ChartModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent{

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
    private router: Router
  ) {
    this.buildBarChart();
    //this.buildDailySalesChart();
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

    if (!sales.length && !purchases.length) return;

    // 1️⃣ Generate all days of current month
    const daysInMonth = new Date(2026, 1, 0).getDate(); // Jan 2026 = 31
    const labels: string[] = [];
    const salesMap = new Map<string, number>();
    const purchaseMap = new Map<string, number>();

    // 2️⃣ Convert API data into maps
    sales.forEach(s =>
      salesMap.set(s.date.substring(0, 10), s.totalAmount)
    );

    purchases.forEach(p =>
      purchaseMap.set(p.date.substring(0, 10), p.totalAmount)
    );

    // 3️⃣ Build aligned arrays
    const salesData: number[] = [];
    const purchaseData: number[] = [];

    for (let day = 1; day <= daysInMonth; day++) {
      const date = `2026-01-${day.toString().padStart(2, '0')}`;
      labels.push(day.toString());

      salesData.push(salesMap.get(date) ?? 0);
      purchaseData.push(purchaseMap.get(date) ?? 0);
    }

    // 4️⃣ Chart.js data
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
