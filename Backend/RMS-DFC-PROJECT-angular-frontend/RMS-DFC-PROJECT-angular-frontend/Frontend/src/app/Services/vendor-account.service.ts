import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class VendorAccountService {
private baseUrll = environment.apiBaseUrl;
  private baseUrl = this.baseUrll + '/vendor-account';

  constructor(private http: HttpClient,private authservice:AuthService) {}

  getVendorAccount(vendorId: number): Observable<any> {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.get(`${this.baseUrl}/${vendorId}`,{headers:reqHeader});
  }

  payVendor(model: any): Observable<any> {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    return this.http.post(`${this.baseUrl}/pay` ,model, {
    responseType: 'text' ,headers:reqHeader  // 👈 same fix as earlier
  });
  }
}
