import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Employee, EmployeeService } from '../../../Services/employee';
import { ToastService } from '../../../Services/toast.service';
import { Router, RouterModule, RoutesRecognized } from '@angular/router';


@Component({
  selector: 'app-employee-form',
  standalone: true,
  imports: [CommonModule, FormsModule,ReactiveFormsModule,RouterModule],
  templateUrl: './employee-form.html',
  styleUrls: ['./employee-form.css']
})
export class EmployeeFormComponent {
  model: any = {
    salaryType: 'Daily'
  };
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


  constructor(private service: EmployeeService,private router:Router ,private toast: ToastService) {}

  save() {
  if (this.form.invalid) {
    this.toast.warn("Entries are missing");
    return;
  }

  // form.value now matches Employee interface
  this.service.create(this.form.value as Employee).subscribe(() => {
    this.toast.success("Employee Created Successfully");
    this.router.navigate(['/emp-list']);
  });
}

 

}
