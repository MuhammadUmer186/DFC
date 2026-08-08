import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

export interface KitchenOutItem {
  rawItemId: number;
  rawItemName: string;
  quantity: number;
}

export interface KitchenOut {
  id: number;
  referenceNo?: string;
  issuedAt: string;
  items: KitchenOutItem[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
}

@Injectable({
  providedIn: 'root'
})
export class KitchenOutService {
  private baseUrll = environment.apiBaseUrl;
  private baseUrl = this.baseUrll + '/kitchen-out';

  constructor(private http: HttpClient, private authservice: AuthService) {}

  // Get all KitchenOut records
  getAll(): Observable<KitchenOut[]> {
    const reqHeader = new HttpHeaders({
      'Authorization': 'Bearer ' + this.authservice.gettoken()
    });
    return this.http.get<KitchenOut[]>(this.baseUrl, { headers: reqHeader });
  }

  // ⭐ Get paged KitchenOut by date
  getPagedByDate(date: string, page: number, pageSize: number): Observable<PagedResult<KitchenOut>> {
    const reqHeader = new HttpHeaders({
      'Authorization': 'Bearer ' + this.authservice.gettoken()
    });
    return this.http.get<PagedResult<KitchenOut>>(
      `${this.baseUrl}/paged-by-date?date=${date}&page=${page}&pageSize=${pageSize}`,
      { headers: reqHeader }
    );
  }

  // Create a KitchenOut
  create(data: any): Observable<any> {
    const reqHeader = new HttpHeaders({
      'Authorization': 'Bearer ' + this.authservice.gettoken()
    });
    return this.http.post(`${this.baseUrl}`, data, {
      responseType: 'text',
      headers: reqHeader
    });
  }

  // Update a KitchenOut
  update(id: number, data: any): Observable<any> {
    const reqHeader = new HttpHeaders({
      'Authorization': 'Bearer ' + this.authservice.gettoken()
    });
    return this.http.put(`${this.baseUrl}/${id}`, data, { headers: reqHeader });
  }

  // Delete a KitchenOut
  delete(id: number): Observable<any> {
    const reqHeader = new HttpHeaders({
      'Authorization': 'Bearer ' + this.authservice.gettoken()
    });
    return this.http.delete(`${this.baseUrl}/${id}`, { headers: reqHeader });
  }
}
