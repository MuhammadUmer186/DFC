import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Category } from '../Models/category.model';
import { MenuItem } from '../Models/menu-item.model';
import { AuthService } from './auth.service';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
export interface MenuItemStats {
  menuItemId: number;
  name: string;
  todayCount: number;
  weekCount?: number;
  monthCount?: number;
}

@Injectable({ providedIn: 'root' })
export class MenuService {
  private baseUrll = environment.apiBaseUrl;
  private apiUrl = this.baseUrll; // update port

  constructor(private http: HttpClient,private authservice:AuthService) {}

  // GET MENU
  getMenu() {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.get<Category[]>(this.apiUrl+'/Menu',{headers:reqHeader});
  }
  getMenuItemStatsByDate(date: string): Observable<MenuItemStats[]> {
    const reqHeader = new HttpHeaders({
      'Authorization': 'Bearer ' + this.authservice.gettoken()
    });
    return this.http.get<MenuItemStats[]>(`${this.apiUrl}/Menu/menu-stats-date?date=${date}`, { headers: reqHeader });
  }

  // CREATE MENU ITEM
  createMenu(item: MenuItem) {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.post<{ id: number }>(this.apiUrl+'/Menu', item,{headers:reqHeader});
  }

  // UPDATE MENU ITEM
  updateMenu(id: number, item: MenuItem) {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.put(`${this.apiUrl+'/Menu'}/${id}`, item,{headers:reqHeader});
  }
  getByCategory(categoryId: number) {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
  return this.http.get<any[]>(`${this.apiUrl}/Menu/ByCategory/${categoryId}`,{headers:reqHeader});
}
  getAll() {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.get<any[]>(  this.baseUrll+'/menuitems',{headers:reqHeader});
  }



  // DELETE MENU ITEM
  deleteMenu(id: number) {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.delete(`${this.apiUrl+'/Menu'}/${id}`,{headers:reqHeader});
  }

  // UPLOAD MENU ITEM IMAGE
  uploadImage(id: number, file: File) {
    const reqHeader = new HttpHeaders({ 'Authorization': 'Bearer ' + this.authservice.gettoken() });
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ imageUrl: string }>(`${this.apiUrl}/Menu/${id}/image`, formData, { headers: reqHeader });
  }
}
