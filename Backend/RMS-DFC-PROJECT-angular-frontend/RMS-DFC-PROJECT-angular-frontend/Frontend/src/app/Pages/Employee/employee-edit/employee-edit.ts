import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Employee, EmployeeService } from '../../../Services/employee';
import { ToastService } from '../../../Services/toast.service';

@Component({
  selector: 'app-employee-edit',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './employee-edit.html',
  styleUrls: ['./employee-edit.css']
})
export class EmployeeEditComponent {

  editId!: number;                     // ID to search
  employeeLoaded = signal(false);      // signal for showing the form

  // Reactive form
  form = new FormGroup({
    name: new FormControl('', { nonNullable: true, validators: Validators.required }),
    mobileNumber: new FormControl('', { nonNullable: true, validators: Validators.required }),
    designation: new FormControl('', { nonNullable: true, validators: Validators.required }),
    nationalId: new FormControl('', { nonNullable: true, validators: Validators.required }),
    address: new FormControl('', { nonNullable: true, validators: Validators.required }),
    salaryType: new FormControl(1, { nonNullable: true, validators: Validators.required }), // 1 = Daily, 2 = Monthly
    salaryAmount: new FormControl(0, { nonNullable: true, validators: Validators.required }),
    isActive: new FormControl(true, { nonNullable: true })
  });

  constructor(private service: EmployeeService, private toast: ToastService) {}

  // Load employee by ID
  loadEmployee() {
    if (!this.editId) {
      this.toast.warn("Enter Employee ID");
      return;
    }

    this.service.getById(this.editId).subscribe({
      next: (e: Employee) => {
        this.form.patchValue({
          name: e.name ?? '',
          mobileNumber: e.mobileNumber ?? '',
          nationalId: e.nationalId ?? '',
          address: e.address ?? '',
          salaryType: e.salaryType ?? 1,
          salaryAmount: e.salaryAmount ?? 0,
          isActive: e.isActive ?? true
        });
        this.employeeLoaded.set(true);
        this.toast.info("Employee loaded for edit");
      },
      error: () => {
        this.toast.error("Employee not found");
        this.employeeLoaded.set(false);
      }
    });
  }

  // Update employee
  update() {
    if (this.form.invalid) {
      this.toast.warn("Entries are missing");
      return;
    }

    const payload: Employee = this.form.getRawValue() as Employee;
    this.service.update(this.editId, payload).subscribe(() => {
      this.toast.success("Employee updated successfully");
      this.employeeLoaded.set(false);
      this.form.reset({ salaryType: 1, isActive: true });
    });
  }
}
