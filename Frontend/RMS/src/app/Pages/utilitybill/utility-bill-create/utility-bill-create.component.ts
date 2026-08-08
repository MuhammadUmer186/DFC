import { Component } from '@angular/core';

import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { UtilityBillService } from '../../../Services/utilitybill.service';

@Component({
  selector: 'app-utility-bill-create',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './utility-bill-create.component.html',
  styleUrl: './utility-bill-create.component.css'
})
export class UtilityBillCreateComponent {

  loading = false;
  successMessage = '';
  errorMessage = '';

  form!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private service: UtilityBillService,
    private router: Router
  ) {
    this.form = this.fb.group({
      billType: ['', Validators.required],
      amount: [0, [Validators.required, Validators.min(1)]],
      billDate: ['', Validators.required],
      dueDate: [''],
      notes: ['']
    });
  }

  submit() {
    if (!this.form.valid) {
      this.errorMessage = "Please fill required fields";
      return;
    }

    this.loading = true;

    this.service.createBill(this.form.value).subscribe({
      next: () => {
        this.successMessage = "Utility Bill Added Successfully";
        this.loading = false;
        setTimeout(() => this.router.navigate(['/dashboard']), 1200);
      },
      error: () => {
        this.errorMessage = "Something went wrong";
        this.loading = false;
      }
    });
  }
}
