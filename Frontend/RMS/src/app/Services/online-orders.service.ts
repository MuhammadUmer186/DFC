import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { OrderQueueDto } from './order-signalr';
import { environment } from '../../environments/environment';

export interface ApproveOrderResult {
  order: OrderQueueDto;
  counterPrinted: boolean;
  kitchenPrinted: boolean;
}

@Injectable({ providedIn: 'root' })
export class OnlineOrdersService {
  private baseUrll = environment.apiBaseUrl;
  private baseURL = this.baseUrll + '/Orders';

  constructor(private http: HttpClient, private authservice: AuthService) {}

  private headers() {
    return new HttpHeaders({ 'Authorization': 'Bearer ' + this.authservice.gettoken() });
  }

  getPending(): Observable<OrderQueueDto[]> {
    return this.http.get<OrderQueueDto[]>(`${this.baseURL}/pending-online`, { headers: this.headers() });
  }

  getActive(): Observable<OrderQueueDto[]> {
    return this.http.get<OrderQueueDto[]>(`${this.baseURL}/active-online`, { headers: this.headers() });
  }

  updateDeliveryStatus(orderId: number, status: 'Preparing' | 'Enroute' | 'Delivered'): Observable<OrderQueueDto> {
    return this.http.patch<OrderQueueDto>(`${this.baseURL}/${orderId}/delivery-status`, { status }, { headers: this.headers() });
  }

  approve(orderId: number): Observable<ApproveOrderResult> {
    return this.http.post<ApproveOrderResult>(`${this.baseURL}/${orderId}/approve`, {}, { headers: this.headers() });
  }

  reject(orderId: number): Observable<OrderQueueDto> {
    return this.http.post<OrderQueueDto>(`${this.baseURL}/${orderId}/reject`, {}, { headers: this.headers() });
  }

  reprintSlip(orderId: number, copy: 'customer' | 'kitchen'): Observable<{ printed: boolean }> {
    return this.http.post<{ printed: boolean }>(`${this.baseURL}/${orderId}/reprint-delivery-slip?copy=${copy}`, {}, { headers: this.headers() });
  }

  assignRider(orderId: number, riderId: number): Observable<OrderQueueDto> {
    return this.http.post<OrderQueueDto>(`${this.baseURL}/${orderId}/assign-rider`, { riderId }, { headers: this.headers() });
  }
}
