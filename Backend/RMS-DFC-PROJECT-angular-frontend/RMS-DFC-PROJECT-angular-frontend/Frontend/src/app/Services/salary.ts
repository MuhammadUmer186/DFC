import { Injectable, signal } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

export interface SalaryStatus {
  employeeId: number;
  employeeName: string;
  mobileNumber: string;
  salaryType: number; // 1 = Daily, 2 = Monthly
  salaryAmount: number;
  isPaid: boolean;
  forDate?: string | null;
  forMonth?: string | null;
}

export interface PaySalaryRequest {
  employeeId: number;
  salaryType: number;
  amountPaid: number;
  forDate?: string | null;
  forMonth?: string | null;
  remarks?: string;
}
export interface SalaryTotalsResponse {
  from: string;
  to: string;
  totalPaid: number;
  totalUnpaid: number;
}
@Injectable({ providedIn: 'root' })
export class SalaryService {
  private baseUrll = environment.apiBaseUrl;
  private api = this.baseUrll + '/salaries';
  salaries = signal<SalaryStatus[]>([]);
  loading = signal(false);

  constructor(private http: HttpClient,private authservice:AuthService) {}

  // GET salary status by date
  getSalaryStatus(date: string): Observable<SalaryStatus[]> {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.get<SalaryStatus[]>(`${this.api}/status?date=${date}`,{headers:reqHeader});
  }

  // POST pay salary
  paySalary(payload: PaySalaryRequest): Observable<any> {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.post(`${this.api}/pay`, payload,{headers:reqHeader});
  }
}
