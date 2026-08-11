import { Component, EventEmitter, Output } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../../Services/auth.service';
import { BrandingService } from '../../../../Services/branding.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css']
})
export class NavbarComponent {
  @Output() toggleSidebar = new EventEmitter<void>();

  roleName = '';

  constructor(private router: Router, public auth: AuthService, public branding: BrandingService) {
    this.roleName = localStorage.getItem('role') ?? '';
    this.branding.load();
  }

  toggle() {
    this.toggleSidebar.emit();
  }

  displayName(): string {
    return this.auth.getEmployeeName() || this.auth.getUserName() || 'User';
  }

  initials(): string {
    const name = this.displayName();
    const parts = name.trim().split(/\s+/).filter(Boolean);
    if (parts.length === 0) return 'U';
    return parts.length === 1
      ? parts[0].substring(0, 2).toUpperCase()
      : (parts[0][0] + parts[1][0]).toUpperCase();
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('role');

    // optional: clear any other auth values
    localStorage.clear();

    this.router.navigate(['/login']);
  }
}
