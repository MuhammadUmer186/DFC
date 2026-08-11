import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class SiteSettingService {
  private api = environment.apiBaseUrl + '/sitesettings';

  constructor(private http: HttpClient, private authservice: AuthService) {}

  get() {
    const reqHeader = new HttpHeaders({ 'Authorization': 'Bearer ' + this.authservice.gettoken() });
    return this.http.get<{ heroImageUrl: string | null; whatsAppNumber: string | null; restaurantName: string | null; logoUrl: string | null }>(this.api, { headers: reqHeader });
  }

  uploadHeroImage(file: File) {
    const reqHeader = new HttpHeaders({ 'Authorization': 'Bearer ' + this.authservice.gettoken() });
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ heroImageUrl: string }>(`${this.api}/hero-image`, formData, { headers: reqHeader });
  }

  updateWhatsAppNumber(whatsAppNumber: string) {
    const reqHeader = new HttpHeaders({ 'Authorization': 'Bearer ' + this.authservice.gettoken() });
    return this.http.put<{ whatsAppNumber: string | null }>(`${this.api}/whatsapp-number`, { whatsAppNumber }, { headers: reqHeader });
  }

  updateRestaurantName(restaurantName: string) {
    const reqHeader = new HttpHeaders({ 'Authorization': 'Bearer ' + this.authservice.gettoken() });
    return this.http.put<{ restaurantName: string | null }>(`${this.api}/restaurant-name`, { restaurantName }, { headers: reqHeader });
  }

  uploadLogo(file: File) {
    const reqHeader = new HttpHeaders({ 'Authorization': 'Bearer ' + this.authservice.gettoken() });
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ logoUrl: string }>(`${this.api}/logo`, formData, { headers: reqHeader });
  }
}
