import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class KitchenOrderService {
  private baseUrll = environment.apiBaseUrl;
  private baseUrl = this.baseUrll + '/api';

  constructor(private http: HttpClient,private authservice:AuthService) {}

  getCategories(): Observable<any[]> {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.get<any[]>(`${this.baseUrll}/categories`,{headers:reqHeader});
  }

  getMenuItems(categoryId: number): Observable<any[]> {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.get<any[]>(`${this.baseUrll}/Menu/ByCategory/${categoryId}`,{headers:reqHeader});
  }

  placeOrder(order: any, discount: number): Observable<any> {
  const reqHeader = new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()});
  return this.http.post(
    `${this.baseUrll}/orders?discount=${discount}`,
    order, { headers: reqHeader }
  );
}

getDeals() {
  const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
  return this.http.get<any[]>(  this.baseUrll + '/Deals',{headers:reqHeader});
}
searchMenuItems(term: string) {
  const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
  return this.http.get<any[]>(
    `${this.baseUrll}/Menu/search?term=${term}`, {headers:reqHeader}
  );
}
}
