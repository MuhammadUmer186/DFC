import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

export interface Vendor {
  name: string;
  phone: string;
  address: string;
}
export interface Vendorupdate {
  id:number;
  name: string;
  phone: string;
  address: string;
}
@Injectable({
  providedIn: 'root'
})
export class VendorService {
private baseUrll = environment.apiBaseUrl;
  private api = this.baseUrll + '/vendors';

  constructor(private http: HttpClient,private authservice:AuthService) {}

  getAll(): Observable<Vendor[]> {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.get<Vendor[]>(this.api,{headers:reqHeader});
  }

  create(data: Vendor): Observable<any> {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.post(this.api, data,{headers:reqHeader});
  }

  update(id: number, data: Vendor): Observable<any> {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.put(`${this.api}/${id}`, data,{headers:reqHeader});
  }

  delete(id: number): Observable<any> {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.delete(`${this.api}/${id}`,{headers:reqHeader});
  }
}
