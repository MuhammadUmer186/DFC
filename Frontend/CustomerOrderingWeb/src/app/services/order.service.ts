import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface PlaceOrderRequest {
  customerName: string;
  phoneNumber: string;
  address: string;
  latitude?: number;
  longitude?: number;
  paymentMethod: 'Cash' | 'Online transfer';
  serviceType: 'DineIn' | 'Takeaway' | 'Delivery';
  areaId?: number;
  items: { menuItemId: number; quantity: number }[];
  deals: { dealId: number; quantity: number }[];
}

export interface PlaceOrderResponse {
  id: number;
  orderNumber?: string | null;
  totalAmount: number;
  status: number;
  serviceType?: string;
  areaName?: string;
  deliveryFeeCharged?: number;
  items: { menuItemName: string; unitPrice: number; quantity: number }[];
  deals: { dealName: string; dealPrice: number; quantity: number }[];
}

export interface OrderStatusResponse {
  id: number;
  orderNumber?: string | null;
  statusLabel: string;
  createdAt: string;
  totalAmount: number;
  serviceType?: string;
  items: { menuItemName: string; unitPrice: number; quantity: number }[];
  deals: { dealName: string; dealPrice: number; quantity: number }[];
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  private baseUrl = environment.apiBaseUrl + '/Public';

  constructor(private http: HttpClient) {}

  placeOrder(request: PlaceOrderRequest): Observable<PlaceOrderResponse> {
    return this.http.post<PlaceOrderResponse>(`${this.baseUrl}/order`, request);
  }

  getOrderStatus(orderId: number, phone: string): Observable<OrderStatusResponse> {
    return this.http.get<OrderStatusResponse>(`${this.baseUrl}/order/${orderId}/status`, { params: { phone } });
  }
}
