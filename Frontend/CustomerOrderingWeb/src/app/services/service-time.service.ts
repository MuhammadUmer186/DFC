import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { ServiceTimeSetting } from '../models/service-time.model';
import { ServiceType } from './order-type.service';

@Injectable({ providedIn: 'root' })
export class ServiceTimeService {
  private baseUrl = environment.apiBaseUrl + '/Public';

  settings = signal<ServiceTimeSetting[]>([]);

  constructor(private http: HttpClient) {
    this.load();
  }

  load() {
    this.http.get<ServiceTimeSetting[]>(`${this.baseUrl}/service-times`).subscribe({
      next: (res) => this.settings.set(res),
      error: () => {}
    });
  }

  label(serviceType: ServiceType): string {
    const setting = this.settings().find(s => s.serviceType === serviceType);
    if (!setting) return '';
    return `${setting.minMinutes}–${setting.maxMinutes} min`;
  }
}
