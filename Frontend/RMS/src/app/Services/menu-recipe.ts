import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

export interface RecipeItem {
  rawItemId: number;
  rawItemName?: string;
  quantityRequired: number;
  unit?: string;
}

export interface MenuRecipeResponse {
  rawItemId: number;
  rawItemName: string;
  quantityRequired: number;
  unit: string;
}

export interface AssignRecipeDto {
  menuItemId: number;
  recipeItems: { rawItemId: number; quantityRequired: number }[];
}

export interface RawItemLite { id: number; name: string; unit: string; }

export interface RecipeOverviewItem {
  menuItemId: number;
  menuItemName: string;
  price: number;
  isAvailable: boolean;
  ingredientCount: number;
}
export interface RecipeOverviewCategory {
  categoryId: number;
  categoryName: string;
  itemCount: number;
  itemsWithRecipe: number;
  items: RecipeOverviewItem[];
}

export interface KitchenAuditRow {
  rawItemId: number;
  rawItemName: string;
  unit: string;
  expectedFromSales: number;
  actualConsumed: number;
  variance: number;
}
export interface KitchenAuditDish {
  menuItemId: number;
  menuItemName: string;
  unitsSold: number;
  hasRecipe: boolean;
  ingredients: KitchenAuditRow[];
}
export interface KitchenAuditReport {
  fromUtc: string;
  toUtc: string;
  ordersCounted: number;
  lineUnitsCounted: number;
  dishesWithoutRecipe: number;
  totals: KitchenAuditRow[];
  byDish: KitchenAuditDish[];
}

@Injectable({ providedIn: 'root' })
export class MenuRecipeService {
  private base = environment.apiBaseUrl;              // '/api' in prod, 'http://localhost:7122/api' in dev
  private api = this.base + '/menu-recipe';

  constructor(private http: HttpClient, private authService: AuthService) {}

  private get headers() {
    return new HttpHeaders({ Authorization: 'Bearer ' + this.authService.gettoken() });
  }

  getOverview(): Observable<RecipeOverviewCategory[]> {
    return this.http.get<RecipeOverviewCategory[]>(`${this.api}/overview`, { headers: this.headers });
  }

  getRecipeByMenuItemId(menuItemId: number): Observable<MenuRecipeResponse[]> {
    return this.http.get<MenuRecipeResponse[]>(`${this.api}/${menuItemId}`, { headers: this.headers });
  }

  assignRecipe(dto: AssignRecipeDto): Observable<any> {
    return this.http.post(`${this.api}/assign`, dto, { headers: this.headers });
  }

  deleteRecipe(menuItemId: number): Observable<any> {
    return this.http.delete(`${this.api}/${menuItemId}`, { headers: this.headers });
  }

  getKitchenAudit(fromIso: string, toIso: string): Observable<KitchenAuditReport> {
    const params = `?from=${encodeURIComponent(fromIso)}&to=${encodeURIComponent(toIso)}`;
    return this.http.get<KitchenAuditReport>(`${this.api}/kitchen-audit${params}`, { headers: this.headers });
  }

  getAllRawItems(): Observable<RawItemLite[]> {
    return this.http.get<RawItemLite[]>(`${this.base}/raw-items`, { headers: this.headers });
  }
}
