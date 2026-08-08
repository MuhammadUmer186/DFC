import { Component, OnInit, signal } from '@angular/core';

import { ReactiveFormsModule, FormBuilder, Validators, FormArray, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';

import { VendorService } from '../../../Services/vendor.service';
import { RawItemService } from '../../../Services/rawitem.service';
import { PurchaseOrderService } from '../../../Services/purchaseorder.service';
import { ToastService } from '../../../Services/toast.service';


@Component({
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './purchaseorder-maincreate.component.html',
  styleUrls: ['./purchaseorder-maincreate.component.css']
})
export class PurchaseOrderMainCreateComponent{

  vendors=signal<any[]>([]);
  rawItems=signal<any[]>([]);
  form!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private vendorService: VendorService,
    private rawItemService: RawItemService,
    private service: PurchaseOrderService,
    private router: Router,
    private toast: ToastService
  ) {
    this.form = this.fb.group({
      billNo: ['', Validators.required],
      vendorId: [null, Validators.required],
      items: this.fb.array([])
    });

    this.vendorService.getAll().subscribe(res => this.vendors.set(res));
    this.rawItemService.getAll().subscribe(res => this.rawItems.set(res));

    this.addItem();
  }

  

  get items() {
    return this.form.get('items') as FormArray;
  }

  addItem() {
    this.items.push(
      this.fb.group({
        rawItemId: [null, Validators.required],
        quantity: [null, [Validators.required, Validators.min(1)]],
        unitPrice: [null, [Validators.required, Validators.min(1)]]
      })
    );
  }

  removeItem(index: number) {
    this.items.removeAt(index);
  }

  save() {
  if (this.form.invalid) {
    this.form.markAllAsTouched();
    this.toast.warn("Please fill all required fields");
    return;
  }

  this.service.create(this.form.value).subscribe({
    next: () => {
      this.toast.success("Purchase Order Saved & Stock Updated Successfully");
      this.router.navigate(['/purchaseorder-list']);
    },
    error: (err) => {
      console.error(err);

      if (err.status === 400) {
        this.toast.error(err.error);
      } else {
        this.toast.error("Failed to save Purchase Order. Please try again.");
      }
    }
  });
}

}

