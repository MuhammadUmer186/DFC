import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode';
import { environment } from '../../environments/environment';
@Injectable({
  providedIn: 'root'
})
export class AuthService {

  baseURL = environment.apiBaseUrl; // Use the API base URL from environment

  constructor(private http: HttpClient, private router: Router) {}

  // ================= REGISTER =================
  createUser(data: any) {
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.gettoken()})
    return this.http.post(this.baseURL + '/Auth/Register', data);
  }

  // ================= LOGIN =================
  LoginUser(data: any) {
    return this.http.post(this.baseURL + '/Auth/Login', data);
  }

  // Call this from login component after success
  saveLogin(token: string) {
    localStorage.setItem('token', token);

    const decoded: any = jwtDecode(token);

    // Support multiple possible role claim keys
    const role =
      decoded['role'] ||
      decoded['roles'] ||
      decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

    if (role) {
      localStorage.setItem('role', role);
    }
  }

  // ================= HELPERS =================
  IsloggedIn() {
    return localStorage.getItem('token') != null;
  }

  gettoken() {
    return localStorage.getItem('token');
  }

  getRole(): string | null {
    return localStorage.getItem('role');
  }

  // ================= LOGOUT =================
  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('role');
    sessionStorage.removeItem('token');
    this.router.navigate(['/login']);
  }

  // ================= ROLE HELPERS =================
  isAdmin() {
    return this.getRole() === 'Admin';
  }

  isCashier() {
    return this.getRole() === 'Cashier';
  }

  isStoreKeeper() {
    return this.getRole() === 'StoreKeeper';
  }
  getEmployeeId(): number  {
  const token = this.gettoken();
  if (!token) return 0;
  const decoded: any = jwtDecode(token);
  return decoded['EmployeeId'] ? Number(decoded['EmployeeId']) : 0;
}
getUserId(): string | null {
  const token = this.gettoken();
  if (!token) return null;

  const decoded: any = jwtDecode(token);
  return decoded['nameid'] || decoded['id'] || null; // Adjust based on what your backend returns
}

getEmployeeName(): string | null {
  const token = this.gettoken();
  if (!token) return null;
  const decoded: any = jwtDecode(token);
  return decoded['EmployeeName'] ?? null;
}
getUserName(): string | null {
  const token = this.gettoken();
  if (!token) return null;

  const decoded: any = jwtDecode(token);
  return decoded['name'] || decoded['UserName']; // match your backend claim
}

}
