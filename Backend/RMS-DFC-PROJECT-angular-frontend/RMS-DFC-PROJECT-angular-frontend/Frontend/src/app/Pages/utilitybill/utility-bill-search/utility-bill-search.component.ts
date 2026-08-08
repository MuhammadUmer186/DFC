import { Component, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { UtilityBillService } from '../../../Services/utilitybill.service';

@Component({
  selector: 'app-utility-bill-search',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './utility-bill-search.component.html',
  styleUrls: ['./utility-bill-search.component.css']
})
export class UtilityBillSearchComponent {

  // Signals
  loading = signal(false);
  errorMessage = signal('');
  bill = signal<any | null>(null);

  form: FormGroup;

  constructor(
    private fb: FormBuilder,
    private service: UtilityBillService
  ) {
    this.form = this.fb.group({
      billDate: ['', Validators.required]
    });
  }

  searchBill() {
    // Reset
    this.errorMessage.set('');
    this.bill.set(null);

    if (!this.form.valid) {
      this.errorMessage.set("Please select bill date");
      return;
    }

    const date = this.form.value.billDate as string;

    this.loading.set(true);

    this.service.getBillByDate(date).subscribe({
      next: (res) => {
        this.bill.set(res);          // ✅ set signal
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set("No bill found for this date");
        this.loading.set(false);
      }
    });
  }
}
