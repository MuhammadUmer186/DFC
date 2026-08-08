import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

//export interface PurchaseOrder {
 // id: number;
 // vendorId: number;
 // rawItemId: number;
 // quantity: number;
 // price: number;
 // totalAmount: number;
 // orderDate: string;
//}
export interface PurchaseOrderItem {
  rawItemId: number;
  rawItemName: string;
  unitPrice: number;
  quantity: number;
  totalPrice: number;
}

export interface PurchaseOrder {
  id: number;
  billNo: string;
  purchaseDate: string;
  vendorId: number;
  vendorName: string;
  totalAmount: number;
  items: PurchaseOrderItem[];
}


@Injectable({
  providedIn: 'root'
})
export class PurchaseOrderService {
  private baseUrll = environment.apiBaseUrl;
  private api = this.baseUrll + '/purchase-orders';
  private sapi=this.baseUrll + '/stock';

  constructor(private http: HttpClient,private authservice:AuthService) {}

  getAll(): Observable<PurchaseOrder[]> {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()}) 
    return this.http.get<PurchaseOrder[]>(this.api,{headers:reqHeader});
  }

  create(data: PurchaseOrder): Observable<any> {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()}) 
  return this.http.post(this.api, data, {
    responseType: 'text'   // 👈 important
  ,headers:reqHeader});
}


update(id: number, data: PurchaseOrder): Observable<any> {
  const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()}) 
    return this.http.put(`${this.api}/${id}`, data,{headers:reqHeader});
}

delete(id: number): Observable<any> {
  const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()}) 
    return this.http.delete(`${this.api}/${id}`,{headers:reqHeader});
}

getStockSummary(){
  const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()}) 
    return this.http.get<any[]>(this.sapi+'/summary',{headers:reqHeader});
 //return this.http.get<any[]>('https://192.168.0.101:7122/api/stock/summary');
}
getByDate(date: string): Observable<PurchaseOrder[]> {

  const reqHeader = new HttpHeaders({
    'Authorization': 'Bearer ' + this.authservice.gettoken()
  });

  return this.http.get<PurchaseOrder[]>(
    `${this.api}/by-date?date=${date}`,
    { headers: reqHeader }
  );

}

}
