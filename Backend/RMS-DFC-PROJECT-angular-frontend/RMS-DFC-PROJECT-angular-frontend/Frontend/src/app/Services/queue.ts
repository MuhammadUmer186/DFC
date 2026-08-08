// queue.service.ts
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Order } from './order.service';
import { AuthService } from './auth.service';
import { OrderQueueDto } from './order-signalr';
import { API_CONFIG } from '../Models/config';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class QueueService {
  // Use environment API base URL
  private baseUrll = environment.apiBaseUrl;
  private baseURL = this.baseUrll + '/Orders';

  constructor(private http: HttpClient,private authservice:AuthService) {}

  /**
   * Get all queued orders
   */
  getQueuedOrders(): Observable<OrderQueueDto[]> {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()}) 
    return this.http.get<OrderQueueDto[]>(`${this.baseURL}/queue`,{headers:reqHeader});
  }

  /**
   * Mark order as paid
   * @param orderId - Order ID
   * @param paymentMethod - 'Cash' or 'Card' etc.
   * @param cashierId - ID of logged-in cashier
   */
  payOrder(orderId: number, paymentMethod: string, cashierId: number): Observable<Order> {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()}) 
    return this.http.post<Order>(`${this.baseURL}/Pay`, {
      orderId,
      paymentMethod,
      cashierId
    },{headers:reqHeader});
  }

  /**
   * Cancel an order
   * @param orderId - Order ID
   * @param cashierId - ID of logged-in cashier
   */
  cancelOrder(orderId: number, cashierId: number): Observable<Order> {
  const headers = new HttpHeaders({
    Authorization: 'Bearer ' + this.authservice.gettoken()
  });

  return this.http.post<Order>(
    `${this.baseURL}/cancel/${orderId}`,
    { cashierId }, // body ONLY what backend expects
    { headers }
  );
}

}
