import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SalaryService, SalaryStatus, PaySalaryRequest } from '../../../Services/salary';
import { ToastService } from '../../../Services/toast.service';

@Component({
  selector: 'app-salary-pay',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './salary-pay.html',
  styleUrls: ['./salary-pay.css']
})
export class SalaryPayComponent {

  // date signal
  selectedDate = signal<string>(new Date().toISOString().slice(0, 10));

  constructor(
    private service: SalaryService,
    private toast: ToastService
  ) {}

  // ✅ expose signals safely
  salaries() {
    return this.service.salaries();
  }

  loading() {
    return this.service.loading();
  }

  // Load salary status
  loadSalaries() {
    this.service.loading.set(true);

    this.service.getSalaryStatus(this.selectedDate()).subscribe({
      next: (res: SalaryStatus[]) => {   // ✅ typed
        this.service.salaries.set(res);
        this.service.loading.set(false);
      },
      error: () => {
        this.toast.error('Failed to load salary data');
        this.service.loading.set(false);
      }
    });
  }

  // Pay salary
  paySalary(row: SalaryStatus) {
    const today = new Date();

    const payload: PaySalaryRequest = {
      employeeId: row.employeeId,
      salaryType: row.salaryType,
      amountPaid: row.salaryAmount,
      forDate: row.salaryType === 1
        ? today.toISOString().slice(0, 10)
        : undefined,
      forMonth: row.salaryType === 2
        ? today.toISOString().slice(0, 7) // yyyy-MM
        : undefined,
      remarks: 'Salary Paid'
    };

    this.service.paySalary(payload).subscribe({
      next: () => {
        this.toast.success(`Salary paid for ${row.employeeName}`);
        this.loadSalaries(); // refresh
      },
      error: () => {
        this.toast.error('Failed to pay salary');
      }
    });
  }
}
