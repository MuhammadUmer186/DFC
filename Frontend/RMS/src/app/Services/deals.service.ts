import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';
import { CreateDealPayload, Deal } from '../Models/deal.model';

@Injectable({ providedIn: 'root' })
export class DealsService {
  private apiUrl = environment.apiBaseUrl + '/Deals';

  constructor(private http: HttpClient, private authservice: AuthService) {}

  private headers() {
    return new HttpHeaders({ 'Authorization': 'Bearer ' + this.authservice.gettoken() });
  }

  getAll() {
    return this.http.get<Deal[]>(`${this.apiUrl}/all`, { headers: this.headers() });
  }

  create(payload: CreateDealPayload) {
    return this.http.post<Deal>(this.apiUrl, payload, { headers: this.headers() });
  }

  update(id: number, payload: CreateDealPayload) {
    return this.http.put<Deal>(`${this.apiUrl}/${id}`, payload, { headers: this.headers() });
  }

  delete(id: number) {
    return this.http.delete(`${this.apiUrl}/${id}`, { headers: this.headers() });
  }

  toggleActive(id: number) {
    return this.http.post<Deal>(`${this.apiUrl}/${id}/toggle-active`, {}, { headers: this.headers() });
  }

  uploadImage(id: number, file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ imageUrl: string }>(`${this.apiUrl}/${id}/image`, formData, { headers: this.headers() });
  }
}
