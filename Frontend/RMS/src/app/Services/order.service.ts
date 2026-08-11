import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { API_CONFIG } from '../Models/config';
import { environment } from '../../environments/environment';

export interface OrderItem {
  menuItemId: number;
  menuItemName: string;
  unitPrice: number;
  quantity: number;
}

export interface DealMenuItem {
  menuItemId: number;
  name: string;
  price: number;
  quantity: number;
}

export interface OrderDeal {
  dealId: number;
  dealName: string;
  dealPrice: number;
  quantity: number;
  items: DealMenuItem[];
}

export interface Order {
  id: number;
  totalAmount: number;
  createdAt: string;           // ISO string
  paid: boolean;
  status: number;              // OrderStatus enum as number (Queued=1, Paid=2, Cancelled=3)
  takenByEmployeeId?: number;
  takenByEmployeeName?: string;
  cashierId?: number;
  cashierName?: string;
  cashierUserName?: string;
  cancelledByEmployeeName?: string;
  cancelledByUserName?: string;
  rejectReason?: string;
  orderSource?: string;
  deliveryStatus?: 'Approved' | 'Preparing' | 'Enroute' | 'Delivered' | 'Rejected' | null;
  items: OrderItem[];
  deals: OrderDeal[];
  totalQuantity: number;
  expanded?: boolean;          // for UI expand/collapse
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
}


@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private baseUrll = environment.apiBaseUrl;
  private baseUrl = this.baseUrll + '/Orders';   // <-- change to your API

  constructor(private http: HttpClient,private authservice:AuthService) {}

  getOrders(): Observable<Order[]> {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.get<Order[]>(this.baseUrl,{headers:reqHeader});
  }
  getPagedOrders(page: number, pageSize: number) {
  const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
  return this.http.get<PagedResult<Order>>(
    this.baseUrll + `/Orders?page=${page}&pageSize=${pageSize}`
  ,{headers:reqHeader});
}

getOrderById(orderId: number): Observable<Order> {
  const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.get<Order>(
      `${this.baseUrl}/${orderId}`,
      { headers: reqHeader }
    );
  }
getPagedOrdersByDate(date: string, page: number, pageSize: number): Observable<PagedResult<Order>> {

  const reqHeader = new HttpHeaders({
    'Authorization': 'Bearer ' + this.authservice.gettoken()
  });

  return this.http.get<PagedResult<Order>>(
    `${this.baseUrl}/paged?date=${date}&page=${page}&pageSize=${pageSize}`,
    { headers: reqHeader }
  );
}

}
