import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

export interface ForecastValue {
  forecastDate: string;
  hourOfDay: number | null;
  menuItemId: number | null;
  menuItemName: string | null;
  predictedSales: number;
  predictedOrderCount: number;
  predictedQuantity: number;
  confidenceLow: number;
  confidenceHigh: number;
  lowConfidence: boolean;
  actualSales: number | null;
  actualOrderCount: number | null;
}

export interface ForecastRun {
  id: number;
  createdAt: string;
  modelVersion: string;
  forecastFrom: string;
  forecastTo: string;
  status: string;
  notes: string | null;
  mae: number | null;
  wape: number | null;
  values: ForecastValue[];
}

export interface ForecastRunSummary {
  id: number;
  createdAt: string;
  forecastFrom: string;
  forecastTo: string;
  mae: number | null;
  wape: number | null;
}

@Injectable({ providedIn: 'root' })
export class AiForecastService {
  private api = environment.apiBaseUrl + '/ai/forecast';

  constructor(private http: HttpClient, private auth: AuthService) {}

  private headers() {
    return new HttpHeaders({ 'Authorization': 'Bearer ' + this.auth.gettoken() });
  }

  getLatest(): Observable<ForecastRun> {
    return this.http.get<ForecastRun>(`${this.api}/latest`, { headers: this.headers() });
  }

  getRuns(): Observable<ForecastRunSummary[]> {
    return this.http.get<ForecastRunSummary[]>(`${this.api}/runs`, { headers: this.headers() });
  }

  recalculate(horizonDays = 7): Observable<ForecastRun> {
    return this.http.post<ForecastRun>(`${this.api}/recalculate?horizonDays=${horizonDays}`, {}, { headers: this.headers() });
  }
}
