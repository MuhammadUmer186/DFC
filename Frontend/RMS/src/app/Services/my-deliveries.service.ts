import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { OrderQueueDto } from './order-signalr';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class MyDeliveriesService {
  private baseUrll = environment.apiBaseUrl;
  private baseURL = this.baseUrll + '/Orders';

  constructor(private http: HttpClient, private authservice: AuthService) {}

  getMyDeliveries(): Observable<OrderQueueDto[]> {
    const reqHeader = new HttpHeaders({ 'Authorization': 'Bearer ' + this.authservice.gettoken() });
    return this.http.get<OrderQueueDto[]>(`${this.baseURL}/my-deliveries`, { headers: reqHeader });
  }

  markDelivered(orderId: number): Observable<OrderQueueDto> {
    const reqHeader = new HttpHeaders({ 'Authorization': 'Bearer ' + this.authservice.gettoken() });
    return this.http.patch<OrderQueueDto>(`${this.baseURL}/${orderId}/delivery-status`, { status: 'Delivered' }, { headers: reqHeader });
  }

  confirmPayment(orderId: number): Observable<OrderQueueDto> {
    const reqHeader = new HttpHeaders({ 'Authorization': 'Bearer ' + this.authservice.gettoken() });
    return this.http.post<OrderQueueDto>(`${this.baseURL}/${orderId}/confirm-payment`, {}, { headers: reqHeader });
  }
}
