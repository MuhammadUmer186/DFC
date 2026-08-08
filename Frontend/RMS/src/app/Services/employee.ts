import { Injectable, signal } from '@angular/core';
import { HttpClient, HttpHeaderResponse, HttpHeaders } from '@angular/common/http';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';


export interface Employee {            // only present when reading from backend
  name: string;
  mobileNumber: string;
  nationalId: string;
  address: string;
  salaryType: number;    // 1 = Daily, 2 = Monthly
  salaryAmount: number;
  isActive: boolean;
}

// Payload type for create/update (no id required)
export type EmployeeRequest = Omit<Employee, 'id'>;



@Injectable({ providedIn: 'root' })
export class EmployeeService {
  private baseUrll = environment.apiBaseUrl;
  private api = this.baseUrll + '/employees';
  constructor(private http: HttpClient,private authservice:AuthService){}

  employees = signal<Employee[]>([]);
  loading = signal(false);



  loadAll() {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.get<any[]>(this.api,{headers:reqHeader});
    
  }

  getById(id: number) {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.get<Employee>(`${this.api}/${id}`,{headers:reqHeader});
  }

  create(data: Employee) {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.post<Employee>(this.api, data,{headers:reqHeader});
  }

  update(id: number, data: Employee) {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.put<Employee>(`${this.api}/${id}`, data,{headers:reqHeader});
  }

  delete(id: number) {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.delete(`${this.api}/${id}`,{headers:reqHeader});
  }
}
