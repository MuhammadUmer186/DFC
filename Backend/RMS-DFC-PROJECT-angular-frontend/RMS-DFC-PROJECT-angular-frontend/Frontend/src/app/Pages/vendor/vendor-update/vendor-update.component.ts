import { Component } from '@angular/core';

import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { VendorService, Vendor, Vendorupdate } from '../../../Services/vendor.service';
import { ToastService } from '../../../Services/toast.service';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './vendor-update.component.html'
})
export class VendorUpdateComponent {

  vendor!: Vendorupdate;

  name = new FormControl('', Validators.required);
  phone = new FormControl('', Validators.required);
  address = new FormControl('', Validators.required);

  constructor(private service: VendorService, private router: Router, private toast: ToastService) {

    const state = history.state;
    if (!state || !state.id) {
      this.toast.warn('Invalid Access');
      this.router.navigate(['/vendor']);
      return;
    }

    this.vendor = state;

    this.name.setValue(this.vendor.name);
    this.phone.setValue(this.vendor.phone);
    this.address.setValue(this.vendor.address);
  }

  update() {
    if (this.name.invalid || this.phone.invalid || this.address.invalid) return;

    const updated: Vendorupdate = {
      id: this.vendor.id,
      name: this.name.value!,
      phone: this.phone.value!,
      address: this.address.value!
    };

    this.service.update(this.vendor.id, updated)
      .subscribe(() => this.router.navigate(['/vendor']));
  }
}
