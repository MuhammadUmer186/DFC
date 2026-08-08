import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

export interface RawItem {
  id: number;
  itemName: string;
  unit: string;
  
}

@Injectable({
  providedIn: 'root'
})
export class RawItemService {
  private baseUrll = environment.apiBaseUrl;
  private api = this.baseUrll + '/raw-items';

  constructor(private http: HttpClient,private authservice:AuthService) {}

  getAll(): Observable<RawItem[]> {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.get<RawItem[]>(this.api,{headers:reqHeader});
  }

  create(data: any): Observable<any> {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.post(this.api, data,{headers:reqHeader});
  }

  update(id: number, data: RawItem): Observable<any> {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.put(`${this.api}/${id}`, data,{headers:reqHeader});
  }

  delete(id: number): Observable<any> {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.delete(`${this.api}/${id}`,{headers:reqHeader});
  }
}
