import { Component, EventEmitter, Output } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../../Services/auth.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css']
})
export class NavbarComponent {
  @Output() toggleSidebar = new EventEmitter<void>();

  roleName = '';

  constructor(private router: Router,public auth:AuthService) {
    this.roleName = localStorage.getItem('role') ?? '';
  }

  toggle() {
    this.toggleSidebar.emit();
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('role');

    // optional: clear any other auth values
    localStorage.clear();

    this.router.navigate(['/login']);
  }
}
