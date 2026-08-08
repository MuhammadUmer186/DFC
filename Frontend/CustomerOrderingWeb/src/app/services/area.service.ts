import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Area {
  id: number;
  name: string;
  deliveryFee: number;
}

@Injectable({ providedIn: 'root' })
export class AreaService {
  private baseUrl = environment.apiBaseUrl + '/Public';

  constructor(private http: HttpClient) {}

  getAreas(): Observable<Area[]> {
    return this.http.get<Area[]>(`${this.baseUrl}/areas`);
  }
}
