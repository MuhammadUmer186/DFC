import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Rider } from '../Models/rider.model';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class RiderService {
  private baseUrll = environment.apiBaseUrl;
  private api = this.baseUrll + '/riders';

  constructor(private http: HttpClient, private authservice: AuthService) {}

  private headers() {
    return new HttpHeaders({ 'Authorization': 'Bearer ' + this.authservice.gettoken() });
  }

  getAll() {
    return this.http.get<Rider[]>(this.api, { headers: this.headers() });
  }

  getById(id: number) {
    return this.http.get<Rider>(`${this.api}/${id}`, { headers: this.headers() });
  }

  create(data: { name: string; phoneNumber: string; vehicleNumber?: string | null }) {
    return this.http.post<Rider>(this.api, data, { headers: this.headers() });
  }

  update(id: number, data: { name: string; phoneNumber: string; vehicleNumber?: string | null; isActive: boolean }) {
    return this.http.put<Rider>(`${this.api}/${id}`, data, { headers: this.headers() });
  }

  delete(id: number) {
    return this.http.delete(`${this.api}/${id}`, { headers: this.headers() });
  }
}
