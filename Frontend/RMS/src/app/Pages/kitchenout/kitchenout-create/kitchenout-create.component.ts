import { Component } from '@angular/core';

import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { KitchenOutService } from '../../../Services/kitchenout.service';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './kitchenout-create.component.html'
})
export class KitchenOutCreateComponent {

  form!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private service: KitchenOutService,
    private router: Router
  ) {
    this.form = this.fb.group({
      itemName: ['', Validators.required],
      quantity: [0, Validators.required],
      unit: ['', Validators.required],
      date: ['', Validators.required]
    });
  }

  save() {
    if (this.form.invalid) return;

    this.service.create(this.form.value)
      .subscribe(() => this.router.navigate(['/kitchenout']));
  }
}
