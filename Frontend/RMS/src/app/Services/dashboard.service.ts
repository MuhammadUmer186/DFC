import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';
export interface DailySummary {
  date: string;
  kitchenOutCost: number;
  wasteCost: number;
  netCost: number;
}
export interface VendorPaymentSummary {
  vendorId: number;
  vendorName: string;
  totalPurchased: number;
  totalPaid: number;
  remainingDue: number;
}

export interface CategorySales {
  categoryName: string;
  totalOrders: number;
}

export interface OrderCountSummary {
  todayOrders: number;
  weeklyOrders: number;
  monthlyOrders: number;
}

export interface StockUsagePercentage {
  kitchenOutPercentage: number;
  wastePercentage: number;
}
export interface utilitybillstotal {
  utilitybill: number;
  
}
export interface DailyReport {
  date: string;

  sales: number;
  purchaseOrdersCost: number;
  kitchenCost: number;
  wasteCost: number;
  salaryPaid: number;
  vendorPayments: number;

  profit: number;

  orders: any[];
  purchaseOrders: any[];
  kitchenOuts: any[];
  wastes: any[];
}



@Injectable({ providedIn: 'root' })
export class DashboardService {
  private baseUrll = environment.apiBaseUrl;
  private baseUrl =  this.baseUrll + '/dashboard';
  private baseUrl2 =  this.baseUrll + '/kitchen-out';
  private baseUrl3 =  this.baseUrll + '/reports';

  constructor(private http: HttpClient,private authservice:AuthService) {}

  getMainSummary() {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.get<any>(`${this.baseUrl}/mainsummary`,{headers:reqHeader});
  }

  getSalesSummary() {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.get<any>(`${this.baseUrl}/sales/summary`,{headers:reqHeader});
  }

  getPurchaseSummary() {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.get<any>(`${this.baseUrl}/purchases/summary`,{headers:reqHeader});
  }

  getStockSummary() {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.get<any[]>(`${this.baseUrl}/StockSummary`,{headers:reqHeader});
  }
  // getTodaySummary(): Observable<DailySummary> {
  //   const today = new Date().toISOString().split('T')[0];
  //   return this.http.get<DailySummary>(`${this.baseUrl}/daily?date=${today}`);
  // }

  getMonthlySummary(year: number, month: number): Observable<DailySummary[]> {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.get<DailySummary[]>(
      `${this.baseUrl}/monthly?year=${year}&month=${month}`,{headers:reqHeader}
    );
  }
  getSalesByDateRange(from: string, to: string) {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
  return this.http.get<any[]>(`${this.baseUrl}/sales-range?from=${from}&to=${to}`,{headers:reqHeader});
}
getSalarystatusByDateRange(from: string, to: string) {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
 return this.http.get<any[]>(`${this.baseUrl3}?from=${from}&to=${to}`,{headers:reqHeader});
}


getPurchasesByDateRange(from: string, to: string) {
  const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
  return this.http.get<any[]>(`${this.baseUrl}/purchases/by-date?from=${from}&to=${to}`,{headers:reqHeader});
}

getDailyProfit(from: string, to: string) {
  const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.get<any[]>(`${this.baseUrl}/daily-profit?from=${from}&to=${to}`,{headers:reqHeader});
}

getProfit(from: string, to: string) {
  const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
  return this.http.get<any>(`${this.baseUrl}/daily-profit?from=${from}&to=${to}`,{headers:reqHeader});
}

// 🔥 Kitchen Summary (Today)
//getKitchenSummary() {
//  return this.getTodaySummary();
//}

// 🔥 Profit Summary (Current Month)
getProfitSummary() {
  const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
  return this.http.get<any>(`${this.baseUrl}/profit/summary`,{headers:reqHeader});
}
getProfitSummarytoday() {
  const today = new Date().toLocaleDateString('en-CA');
  return this.getProfit(today, today);
}
getSalaryStatusSummarytoday() {
  const today = new Date().toLocaleDateString('en-CA');
  return this.getSalarystatusByDateRange(today, today);
}


// 🔥 Daily Sales (last 7 days)
getDailySales() {
  const today = new Date();
  const to = today.toLocaleDateString('en-CA');

  const fromDate = new Date();
  fromDate.setDate(today.getDate() - 7);
  const from = fromDate.toLocaleDateString('en-CA');

  return this.getSalesByDateRange(from, to);
}
getDailyProfits() {
  const today = new Date();
  const to = today.toLocaleDateString('en-CA');

  const fromDate = new Date();
  fromDate.setDate(today.getDate() - 7);
  const from = fromDate.toLocaleDateString('en-CA');

  return this.getProfit(from, to);
}
getDailyPurchases() {
  const today = new Date();
  const to = today.toLocaleDateString('en-CA');

  const fromDate = new Date();
  fromDate.setDate(today.getDate() - 7);
  const from = fromDate.toLocaleDateString('en-CA');

  return this.getPurchasesByDateRange(from, to);
}
getAverageKitchenCosts() {
  const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
  return this.http.get<any[]>(`${this.baseUrl2}/average-costs`,{headers:reqHeader});
}

getDailyKitchenCost(date: string) {
  const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
  return this.http.get<any>(`${this.baseUrl2}/daily?date=${date}`,{headers:reqHeader});
}
getkitchenwastetoday() {
  const today = new Date().toLocaleDateString('en-CA');
  return this.getDailyKitchenCost(today);
}
getVendorPaymentSummary(): Observable<VendorPaymentSummary[]> {
  const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
  return this.http.get<VendorPaymentSummary[]>(
    `${this.baseUrl}/vendors/payment-summary`,{headers:reqHeader}
  );
}
getTopSellingCategories(): Observable<CategorySales[]> {
  const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
  return this.http.get<CategorySales[]>(
    `${this.baseUrl}/sales/top-categories`,{headers:reqHeader}
  );
}
getOrderCountSummary(): Observable<OrderCountSummary> {
  const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
  return this.http.get<OrderCountSummary>(
    `${this.baseUrl}/orders/count-summary`,{headers:reqHeader}
  );
}
getStockUsagePercentage(): Observable<StockUsagePercentage> {
  const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
  return this.http.get<StockUsagePercentage>(
    `${this.baseUrl}/stock/usage-percentage`,{headers:reqHeader}
  );
}
getMonthlyUtilityBills():Observable<utilitybillstotal> {
  const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
  return this.http.get<utilitybillstotal>(
    `${this.baseUrl}/utility`,{headers:reqHeader}
  );
}
getDailyReport(date: string): Observable<DailyReport> {
  const reqHeader = new HttpHeaders({
    'Authorization': 'Bearer ' + this.authservice.gettoken()
  });

  return this.http.get<DailyReport>(
    `${this.baseUrl3}/daily?date=${date}`,
    { headers: reqHeader }
  );
}

getReportByRange(from: string, to: string): Observable<DailyReport> {
  const reqHeader = new HttpHeaders({
    'Authorization': 'Bearer ' + this.authservice.gettoken()
  });

  return this.http.get<DailyReport>(
    `${this.baseUrl3}/range?from=${from}&to=${to}`,
    { headers: reqHeader }
  );
}
}
