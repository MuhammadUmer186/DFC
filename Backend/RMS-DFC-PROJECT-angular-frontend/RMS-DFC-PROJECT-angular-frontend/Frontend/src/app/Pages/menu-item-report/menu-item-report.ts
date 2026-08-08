import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MenuService,MenuItemStats } from '../../Services/menu.service';

@Component({
  selector: 'app-menu-item-report',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './menu-item-report.html',
  styleUrls: ['./menu-item-report.css']
})
export class MenuItemReportComponent {
  date = signal('');
  loading = signal(false);
  menuStats = signal<MenuItemStats[]>([]);

  constructor(private menuItemReportService: MenuService) {}

  loadReport() {
    if (!this.date()) {
      alert('Please select a date');
      return;
    }

    this.loading.set(true);

    this.menuItemReportService.getMenuItemStatsByDate(this.date()).subscribe({
      next: (res) => {
        this.menuStats.set(res);
        this.loading.set(false);
      },
      error: () => {
        alert('Error fetching menu item stats');
        this.loading.set(false);
      }
    });
  }
}
