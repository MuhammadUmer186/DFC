import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class SystemService {
  private baseUrll = environment.apiBaseUrl;
  private api = this.baseUrll + '/system';

  constructor(private http: HttpClient, private authservice: AuthService) {}

  private headers() {
    return new HttpHeaders({ 'Authorization': 'Bearer ' + this.authservice.gettoken() });
  }

  clearData(password: string) {
    return this.http.post<{ message: string }>(`${this.api}/clear-data`, { password }, { headers: this.headers() });
  }
}
