import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { PublicMenu } from '../models/menu.model';

@Injectable({ providedIn: 'root' })
export class MenuService {
  private baseUrl = environment.apiBaseUrl + '/Public';

  constructor(private http: HttpClient) {}

  getMenu(): Observable<PublicMenu> {
    return this.http.get<PublicMenu>(`${this.baseUrl}/menu`);
  }

  fullImageUrl(imageUrl?: string | null): string | null {
    if (!imageUrl) return null;
    return `${environment.apihub}${imageUrl}`;
  }
}
