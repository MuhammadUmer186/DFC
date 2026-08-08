import { Component } from '@angular/core';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { ToastService } from '../../../Services/toast.service';
import { VendorService, Vendor } from '../../../Services/vendor.service';
import { Button, ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-vendor-create',
  standalone: true,
  imports: [ReactiveFormsModule, RouterModule],
  templateUrl: './vendor-create.component.html',
  styleUrls: ['./vendor-create.component.css']
})
export class VendorCreateComponent {
  form = new FormGroup({
    name: new FormControl('', { nonNullable: true, validators: Validators.required }),
    phone: new FormControl('', { nonNullable: true, validators: Validators.required }),
    address: new FormControl('', { nonNullable: true, validators: Validators.required })
  });

  constructor(private service: VendorService, private router: Router, private toast: ToastService) {}

  save() {
    if (this.form.invalid) {
      this.toast.warn("Entries are missing");
      return;
    }

    this.service.create(this.form.value as Vendor).subscribe(() => {
      this.toast.success("Vendor Created Successfully");
      this.router.navigate(['/vendor-create']);
    });
  }
}
