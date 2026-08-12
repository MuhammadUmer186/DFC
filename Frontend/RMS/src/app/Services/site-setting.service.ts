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
    return this.http.get<{
      heroImageUrl: string | null;
      whatsAppNumber: string | null;
      restaurantName: string | null;
      logoUrl: string | null;
      menuPdfUrl: string | null;
      orderSerialPrefix: string;
      orderSerialStartingNumber: number;
      orderSerialResetTime: string;
      country: string;
      timeZoneId: string;
      currencyCode: string;
      currencySymbol: string;
    }>(this.api, { headers: reqHeader });
  }

  updateCountryTimeZone(country: string, timeZoneId: string, currencyCode: string, currencySymbol: string) {
    const reqHeader = new HttpHeaders({ 'Authorization': 'Bearer ' + this.authservice.gettoken() });
    return this.http.put<{ country: string; timeZoneId: string; currencyCode: string; currencySymbol: string }>(
      `${this.api}/country-timezone`,
      { country, timeZoneId, currencyCode, currencySymbol },
      { headers: reqHeader }
    );
  }

  updateOrderSerialSetting(prefix: string, startingNumber: number, resetTime: string) {
    const reqHeader = new HttpHeaders({ 'Authorization': 'Bearer ' + this.authservice.gettoken() });
    return this.http.put<{ orderSerialPrefix: string; orderSerialStartingNumber: number; orderSerialResetTime: string }>(
      `${this.api}/order-serial`,
      { prefix, startingNumber, resetTime },
      { headers: reqHeader }
    );
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

  uploadMenuPdf(file: File) {
    const reqHeader = new HttpHeaders({ 'Authorization': 'Bearer ' + this.authservice.gettoken() });
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ menuPdfUrl: string }>(`${this.api}/menu-pdf`, formData, { headers: reqHeader });
  }
}
