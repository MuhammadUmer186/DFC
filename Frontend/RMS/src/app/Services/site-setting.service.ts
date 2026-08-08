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
    return this.http.get<{ heroImageUrl: string | null }>(this.api, { headers: reqHeader });
  }

  uploadHeroImage(file: File) {
    const reqHeader = new HttpHeaders({ 'Authorization': 'Bearer ' + this.authservice.gettoken() });
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ heroImageUrl: string }>(`${this.api}/hero-image`, formData, { headers: reqHeader });
  }
}
