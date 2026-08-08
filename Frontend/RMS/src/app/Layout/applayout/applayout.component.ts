
import { Component } from '@angular/core';
import { NavbarComponent } from '../Component/navbar/navbar/navbar.component';
import { SidebarComponent } from '../Component/sidebar/sidebar/sidebar.component';
import { RouterOutlet } from '@angular/router';

@Component({
  standalone: true,
  selector: 'app-app-layout',
  imports: [
    NavbarComponent,
    SidebarComponent,
    RouterOutlet
],
  templateUrl: './applayout.component.html',
  styleUrls: ['./applayout.component.css']
})
export class AppLayoutComponent {

  sidebarOpen: boolean = true;

toggleSidebar() {
  this.sidebarOpen = !this.sidebarOpen;
  console.log("Sidebar state:", this.sidebarOpen);
}


}
