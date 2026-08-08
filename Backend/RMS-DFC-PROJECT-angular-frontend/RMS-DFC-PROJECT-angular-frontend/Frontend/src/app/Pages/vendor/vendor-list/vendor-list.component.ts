import { Component, OnInit } from '@angular/core';

import { Router } from '@angular/router';
import { VendorService, Vendor } from '../../../Services/vendor.service';

@Component({
  standalone: true,
  imports: [],
  templateUrl: './vendor-list.component.html'
})
export class VendorListComponent implements OnInit {

  vendors: Vendor[] = [];

  constructor(private service: VendorService, private router: Router) {}

  ngOnInit(): void {
    this.loadVendors();
  }

  loadVendors() {
    this.service.getAll().subscribe(res => this.vendors = res);
  }

  edit(id: number) {
    this.router.navigate(['/vendor/edit', id]);
  }

  deleteVendor(id: number) {
    if (!confirm('Are you sure to delete vendor?')) return;

    this.service.delete(id).subscribe(() => this.loadVendors());
  }
}
