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
    employeeId: new FormControl('', Validators.required)
  });

  constructor(
    private service: AuthService,
    private router: Router,
    private toast: ToastService
  ) {}

  togglePassword() {
    this.showPassword = !this.showPassword;
  }

  OnUserSave() {
    if (this.userform.invalid) return;

    this.service.createUser(this.userform.value).subscribe({
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

