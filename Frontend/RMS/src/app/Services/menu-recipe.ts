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
  recipeItems: RecipeItem[];
}

@Injectable({
  providedIn: 'root'
})
export class MenuRecipeService {
  private baseUrll = environment.apiBaseUrl;
  private api = this.baseUrll + '/api/menu-recipe';

  constructor(private http: HttpClient, private authService: AuthService) {}

  getRecipeByMenuItemId(menuItemId: number): Observable<MenuRecipeResponse[]> {
    const headers = new HttpHeaders({ 'Authorization': 'Bearer ' + this.authService.gettoken() });
    return this.http.get<MenuRecipeResponse[]>(`${this.api}/by-menu-item/${menuItemId}`, { headers });
  }

  assignRecipe(dto: AssignRecipeDto): Observable<any> {
    const headers = new HttpHeaders({ 'Authorization': 'Bearer ' + this.authService.gettoken() });
    return this.http.post(`${this.api}/assign`, dto, { responseType: 'text', headers });
  }

  deleteRecipe(menuItemId: number): Observable<any> {
    const headers = new HttpHeaders({ 'Authorization': 'Bearer ' + this.authService.gettoken() });
    return this.http.delete(`${this.api}/delete/${menuItemId}`, { headers });
  }

  getAllRawItems(): Observable<{ id: number, name: string, unit: string }[]> {
    const headers = new HttpHeaders({ 'Authorization': 'Bearer ' + this.authService.gettoken() });
    return this.http.get<{ id: number, name: string, unit: string }[]>(this.baseUrll + '/raw-items', { headers });
  }

  getAllMenuItems(): Observable<{ id: number, name: string }[]> {
    const headers = new HttpHeaders({ 'Authorization': 'Bearer ' + this.authService.gettoken() });
    return this.http.get<{ id: number, name: string }[]>(this.baseUrll + '/menu-items', { headers });
  }
}
