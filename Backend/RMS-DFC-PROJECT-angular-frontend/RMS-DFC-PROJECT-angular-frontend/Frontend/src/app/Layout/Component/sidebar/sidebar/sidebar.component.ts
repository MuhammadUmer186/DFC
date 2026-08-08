import { Component, Input, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../../Services/auth.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css']
})
export class SidebarComponent implements OnInit {
  @Input() closed = false;

  role: string | null = null;

  /** Currently open dropdown menu */
  openMenu: string | null = null;

  constructor(private auth: AuthService) {}

  ngOnInit(): void {
    this.role = this.auth.getRole();
    console.log("Sidebar Role:", this.role);
  }

  /**
   * Toggle sidebar dropdown menu
   * @param menu The menu to toggle
   */
  toggleMenu(menu: string) {
    // If clicked menu is already open, close it. Otherwise, open the new menu.
    this.openMenu = this.openMenu === menu ? null : menu;
  }
}
