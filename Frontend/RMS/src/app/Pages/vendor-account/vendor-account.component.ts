import { Component, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { VendorAccountService } from '../../Services/vendor-account.service';
import { CommonModule } from '@angular/common';
import { VendorService } from '../../Services/vendor.service';
import { ToastService } from '../../Services/toast.service';
import { RmsDatePipe } from '../../Shared/pipes/rms-date.pipe';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RmsDatePipe],
  selector: 'app-vendor-account',
  templateUrl: './vendor-account.component.html',
  styleUrls: ['./vendor-account.component.css']
})
export class VendorAccountComponent{

  vendors=signal<any[]>([]);
  vendorId=signal<number>(0);

  vendor=signal<any>(null);
  loading=signal<boolean>(false);
  showPaymentForm=signal<boolean>(false);

  paymentForm!: FormGroup;

  constructor(
    private service: VendorAccountService,
    private serv:VendorService,
    private fb: FormBuilder,
    private toast: ToastService
  ) {

    this.paymentForm = this.fb.group({
      vendorId: [''],
      amountPaid: [''],
      paidByUser: [localStorage.getItem('role') || '']
    });

    this.loadVendors();
  }

  

  // 🔹 Load Vendors For Dropdown
  loadVendors() {
    this.serv.getAll().subscribe({
      next: (res) => this.vendors.set(res),
      error: () => this.toast.error('Failed to load vendors')
    });
  }

  // 🔹 Load Vendor Account Details
 loadVendorAccount() {
  // Read the vendorId signal using ()
  if (!this.vendorId()) return;

  this.loading.set(true);
  this.vendor.set(null);

  this.service.getVendorAccount(this.vendorId()).subscribe({
    next: (res) => {
      // Update vendor signal
      this.vendor.set(res);
      this.loading.set(false);
    },
    error: (err) => {
      this.toast.error(err?.error || 'Failed to load vendor account');
      this.loading.set(false);
    }
  });
}

openPaymentForm() {
  // Show payment form
  this.showPaymentForm.set(true);

  // Read vendor signal using ()
  const v = this.vendor();
  if (!v) return;

  // Patch form
  this.paymentForm.patchValue({
    vendorId: v.vendorId   // or v.id depending on your backend
  });
}


  // 🔹 Submit Payment + AUTO Refresh
  submitPayment() {

  const amount = Number(this.paymentForm.value.amountPaid);
  if (!amount || amount <= 0) {
    this.toast.warn('Enter a valid payment amount');
    return;
  }

  this.paymentForm.patchValue({
    paidByUser: localStorage.getItem('role') || ''
  });

  this.service.payVendor(this.paymentForm.value).subscribe({
    next: () => {
      this.toast.success("Payment Recorded Successfully");

      this.showPaymentForm.set(false);
      this.paymentForm.reset();

      // Reload vendor account section
      this.loadVendorAccount();

      // Optional full page reload (if you really want)
      // location.reload();
    },
    error: (err) => {
      console.error(err);

      if (err.status === 400) {
        this.toast.error(err.error);
      } else {
        this.toast.error("Failed to record payment. Please try again.");
      }
    }
  });
}

}
