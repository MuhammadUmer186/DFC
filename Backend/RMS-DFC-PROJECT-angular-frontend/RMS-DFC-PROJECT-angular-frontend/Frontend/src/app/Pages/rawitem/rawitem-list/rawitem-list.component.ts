import { Component, OnInit } from '@angular/core';

import { Router } from '@angular/router';
import { RawItemService, RawItem } from '../../../Services/rawitem.service';

@Component({
  standalone: true,
  imports: [],
  templateUrl: './rawitem-list.component.html'
})
export class RawItemListComponent implements OnInit {

  rawItems: RawItem[] = [];

  constructor(private service: RawItemService, private router: Router) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData() {
    this.service.getAll().subscribe(res => this.rawItems = res);
  }

  edit(item: RawItem) {
    this.router.navigate(['/rawitem/edit', item.id], { state: item });
  }

  delete(id: number) {
    if (!confirm("Delete this Raw Item?")) return;

    this.service.delete(id).subscribe(() => this.loadData());
  }
}
