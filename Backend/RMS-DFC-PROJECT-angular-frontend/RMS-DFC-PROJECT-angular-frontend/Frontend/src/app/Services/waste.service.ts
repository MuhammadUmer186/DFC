import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

export interface WasteItem {
  rawItemId: number;
  rawItemName: string;
  quantity: number;
}

export interface WasteRecord {
  id: number;
  referenceNo: string;
  createdAt: string;
  reason: string;
  items: WasteItem[];
}

@Injectable({
  providedIn: 'root'
})
export class WasteService {
private baseUrll = environment.apiBaseUrl;
  private apiUrl = this.baseUrll + '/Waste';

  constructor(private http: HttpClient,private authservice:AuthService) {}

  getAll(): Observable<WasteRecord[]> {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.get<WasteRecord[]>(this.apiUrl,{headers:reqHeader});
  }
}
