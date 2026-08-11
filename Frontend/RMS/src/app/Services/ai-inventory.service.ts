import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

export interface InventoryRecommendation {
  id: number;
  createdAt: string;
  rawItemId: number;
  rawItemName: string;
  unit: string;
  currentStock: number;
  forecastedDemand: number;
  suggestedReorderQuantity: number;
  suggestedReorderDate: string | null;
  recommendationType: 'LowStock' | 'Reorder' | 'ExcessStock' | 'ExpiryRisk' | 'WasteReduction';
  explanation: string;
  dataWarnings: string | null;
  confidenceLow: number;
  confidenceHigh: number;
  status: 'Pending' | 'Approved' | 'Rejected' | 'Modified';
}

@Injectable({ providedIn: 'root' })
export class AiInventoryService {
  private api = environment.apiBaseUrl + '/ai/inventory-recommendations';

  constructor(private http: HttpClient, private auth: AuthService) {}

  private headers() {
    return new HttpHeaders({ 'Authorization': 'Bearer ' + this.auth.gettoken() });
  }

  getAll(status: string = 'Pending'): Observable<InventoryRecommendation[]> {
    return this.http.get<InventoryRecommendation[]>(`${this.api}?status=${status}`, { headers: this.headers() });
  }

  recalculate(): Observable<InventoryRecommendation[]> {
    return this.http.post<InventoryRecommendation[]>(`${this.api}/recalculate`, {}, { headers: this.headers() });
  }

  decide(id: number, decision: 'Approved' | 'Rejected' | 'Modified', modifiedQuantity?: number, feedback?: string): Observable<InventoryRecommendation> {
    return this.http.post<InventoryRecommendation>(`${this.api}/${id}/decision`, { decision, modifiedQuantity, feedback }, { headers: this.headers() });
  }
}
