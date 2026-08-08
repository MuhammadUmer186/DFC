import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Category } from '../Models/category.model';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';
@Injectable({ providedIn: 'root' })
export class CategoryService {
  private baseUrll = environment.apiBaseUrl;
  private api = this.baseUrll + '/categories';  

  constructor(private http: HttpClient,private authservice:AuthService) {}

  getAll() {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.get<Category[]>(this.api,{headers:reqHeader});
  }

  getById(id: number) {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.get<Category>(`${this.api}/${id}`,{headers:reqHeader});
  }

  create(data: Category) {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.post(this.api, data,{headers:reqHeader});
  }

  update(id: number, data: Category) {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.put(`${this.api}/${id}`, data,{headers:reqHeader});
  }

  delete(id: number) {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.delete(`${this.api}/${id}`,{headers:reqHeader});
  }
}
