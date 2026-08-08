import { Component, OnInit,signal } from '@angular/core';

import { ReactiveFormsModule, FormBuilder, Validators, FormArray, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';

import { RawItemService } from '../../../Services/rawitem.service';
import { KitchenOutService } from '../../../Services/kitchenout.service';
import { ToastService } from '../../../Services/toast.service';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './kitchenout-maincreate.component.html',
  styleUrls: ['./kitchenout-maincreate.component.css']
})
export class KitchenOutMainCreateComponent {

  rawItems=signal<any[]>([]);
  form!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private rawItemService: RawItemService,
    private service: KitchenOutService,
    private router: Router,
    private toast: ToastService
  ) {


    // Initialize Form
    this.form = this.fb.group({
      referenceNo: ['', Validators.required],
      items: this.fb.array([])
    });

    // Load raw items
    this.rawItemService.getAll().subscribe(res => {
      this.rawItems.set(res);
      this.addItem();
    });

  }

  
  get items() {
    return this.form.get('items') as FormArray;
  }

  addItem() {
    this.items.push(
      this.fb.group({
        rawItemId: [null, Validators.required],
        quantity: [null, [Validators.required, Validators.min(1)]]
      })
    );
  }

  removeItem(index: number) {
    this.items.removeAt(index);
  }

  save() {
  if (this.form.invalid) {
    this.toast.error("Please fill all required fields before saving");
    return;
  }

  const payload = {
    referenceNo: this.form.value.referenceNo,
    items: this.items.value.map((x: any) => ({
      rawItemId: x.rawItemId,
      quantity: x.quantity
    }))
  };

  this.service.create(payload).subscribe({
    next: () => {
      this.toast.success("Kitchen Out Saved & Stock Reduced Successfully 👍");
      this.router.navigate(['/kitchenout-list']);
    },
    error: (err) => {
      if (err.status === 400) {
        this.toast.error(err.error);   // <-- will show "Insufficient stock"
      } else {
        this.toast.error("❌ Failed to save kitchen out. Please try again.");
      }
    }
  });
}

}
