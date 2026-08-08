import { Component, OnInit } from '@angular/core';

import { Router } from '@angular/router';

import { MenuService } from '../../../../Services/menu.service';

@Component({
  standalone: true,
  imports: [],
  templateUrl: './menu-list.component.html'
})
export class MenuListComponent implements OnInit {

  menuItems: any[] = [];

  constructor(
    private service: MenuService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.service.getMenu().subscribe(res => {
      this.menuItems = res;
    });
  }

  edit(id: number) {
    this.router.navigate(['/menu/edit', id]);
  }

  delete(id: number) {
    if (!confirm('Delete this menu item?')) return;

    this.service.deleteMenu(id)
      .subscribe(() => this.loadData());
  }
}
