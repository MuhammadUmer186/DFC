import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Client } from '../Models/client.model';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ClientService {
  private baseUrll = environment.apiBaseUrl;
  private api = this.baseUrll + '/customers';

  constructor(private http: HttpClient, private authservice: AuthService) {}

  private headers() {
    return new HttpHeaders({ 'Authorization': 'Bearer ' + this.authservice.gettoken() });
  }

  getAll() {
    return this.http.get<Client[]>(this.api, { headers: this.headers() });
  }
}
