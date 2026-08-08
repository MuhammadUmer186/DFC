import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';
export interface UtilityBill {
  id?: number;
  billType: string;
  amount: number;
  billDate: string;
  dueDate?: string;
  notes?: string;
}


@Injectable({
  providedIn: 'root'
})
export class UtilityBillService {
private baseUrll = environment.apiBaseUrl;
  private apiUrl = this.baseUrll + '/UtilityBills';

  constructor(private http: HttpClient,private authservice:AuthService) {}

  createBill(model: UtilityBill): Observable<any> {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.post(`${this.apiUrl}`, model,{headers:reqHeader});
  }

  getBills(): Observable<any> {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.get(`${this.apiUrl}`,{headers:reqHeader});
  }
  getBillByDate(date: string) {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
  return this.http.get<any[]>(`${this.apiUrl}/${date}`,{headers:reqHeader});
}

}
