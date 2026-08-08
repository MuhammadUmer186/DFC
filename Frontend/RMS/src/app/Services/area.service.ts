import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Area } from '../Models/area.model';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AreaService {
  private baseUrll = environment.apiBaseUrl;
  private api = this.baseUrll + '/areas';

  constructor(private http: HttpClient, private authservice: AuthService) {}

  private headers() {
    return new HttpHeaders({ 'Authorization': 'Bearer ' + this.authservice.gettoken() });
  }

  getAll() {
    return this.http.get<Area[]>(this.api, { headers: this.headers() });
  }

  getById(id: number) {
    return this.http.get<Area>(`${this.api}/${id}`, { headers: this.headers() });
  }

  create(data: { name: string; deliveryFee: number }) {
    return this.http.post<Area>(this.api, data, { headers: this.headers() });
  }

  update(id: number, data: { name: string; deliveryFee: number; isActive: boolean }) {
    return this.http.put<Area>(`${this.api}/${id}`, data, { headers: this.headers() });
  }

  delete(id: number) {
    return this.http.delete(`${this.api}/${id}`, { headers: this.headers() });
  }
}
