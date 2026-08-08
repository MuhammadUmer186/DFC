import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ServiceTimeSetting } from '../Models/service-time-setting.model';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ServiceTimeSettingService {
  private api = environment.apiBaseUrl + '/servicetimesettings';

  constructor(private http: HttpClient, private authservice: AuthService) {}

  getAll() {
    const reqHeader = new HttpHeaders({ 'Authorization': 'Bearer ' + this.authservice.gettoken() });
    return this.http.get<ServiceTimeSetting[]>(this.api, { headers: reqHeader });
  }

  update(serviceType: string, minMinutes: number, maxMinutes: number) {
    const reqHeader = new HttpHeaders({ 'Authorization': 'Bearer ' + this.authservice.gettoken() });
    return this.http.put<ServiceTimeSetting>(`${this.api}/${serviceType}`, { minMinutes, maxMinutes }, { headers: reqHeader });
  }
}
