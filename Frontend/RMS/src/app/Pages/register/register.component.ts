import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { NgOptimizedImage } from '@angular/common';
import { AuthService } from '../../Services/auth.service';
import { ToastService } from '../../Services/toast.service';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterOutlet],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  showPassword = false;

  userform = new FormGroup({
    userName: new FormControl('', [Validators.required, Validators.minLength(5)]),
    password: new FormControl('', [Validators.required, Validators.minLength(5)]),
    roles: new FormControl('', Validators.required),
    employeeId: new FormControl('')
  });

  constructor(
    private service: AuthService,
    private router: Router,
    private toast: ToastService
  ) {}

  get isAdminRole(): boolean {
    return this.userform.controls['roles'].value === 'Admin';
  }

  togglePassword() {
    this.showPassword = !this.showPassword;
  }

  OnUserSave() {
    if (this.userform.invalid) return;

    const payload: any = {
      userName: this.userform.value.userName,
      password: this.userform.value.password,
      roles: this.userform.value.roles
    };

    if (!this.isAdminRole) {
      const employeeId = Number(this.userform.value.employeeId);
      if (!employeeId) {
        this.toast.error('Employee ID is required');
        return;
      }
      payload.employeeId = employeeId;
    }

    this.service.createUser(payload).subscribe({
      next: () => {
        this.toast.success('User registered successfully');
        this.userform.reset();
      },
      error: err => {
        this.toast.error(err.error?.message || 'Registration failed');
      }
    });
  }
}

