import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { AuthService } from '../Services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(private auth: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean | UrlTree {

    // Not logged in → go login
    if (!this.auth.IsloggedIn()) {
      return this.router.parseUrl('/login');
    }

    const allowedRoles = route.data['roles'] as Array<string>;
    const userRole = this.auth.getRole();

    // If no role restriction → allow
    if (!allowedRoles || allowedRoles.length === 0) {
      return true;
    }

    // Role allowed?
    if (allowedRoles.includes(userRole!)) {
      return true;
    }

    // ❌ Unauthorized role → you can redirect wherever you like
    return this.router.parseUrl('/unauthorized');
  }
}
