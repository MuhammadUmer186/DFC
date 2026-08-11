import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from '../../Services/auth.service';
import { ToastService } from '../../Services/toast.service';
import { BrandingService } from '../../Services/branding.service';
//import { ProgressSpinnerComponent } from 'primeng/progressspinner';


@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterOutlet, CommonModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  showPassword = false;
  currentYear = new Date().getFullYear();

  constructor(private service: AuthService, private router: Router, private toast: ToastService, public branding: BrandingService) {}

  loginform: FormGroup = new FormGroup({
    userName: new FormControl("", [Validators.required, Validators.minLength(3)]),
    password: new FormControl("", [Validators.required])
  });

  ngOnInit(): void {
    this.branding.load();
    if (this.service.IsloggedIn()) {

      const role = this.service.getRole();

      if (role === "SuperAdmin") {
        this.router.navigateByUrl("/dashboard");
      }
      if (role === "Admin") {
        this.router.navigateByUrl("/dashboard");
      }
      if (role === "MainAdmin") {
        this.router.navigateByUrl("/dashboard");
      }
      else if (role === "Cashier") {
        this.router.navigateByUrl("/ORDERCREATE");
      }
      else if (role === "Waiter") {
        this.router.navigateByUrl("/ORDERCREATE");
      }
      else if (role === "StoreKeeper") {
        this.router.navigateByUrl("MainPO");
      }
      else if (role === "Rider") {
        this.router.navigateByUrl("/my-deliveries");
      }
    }
  }

  OnloginSave() {
    if (!this.loginform.valid) return;

    this.service.LoginUser(this.loginform.value).subscribe({
      next: (res: any) => {

        if (!res?.token) {
          this.toast.error("Token missing in response!");
          return;
        }

        const token = res.token;

        try {
          const payload = JSON.parse(atob(token.split('.')[1]));
          console.log("JWT Payload:", payload);

          const role =
            payload.role ||
            payload.roles?.[0] ||
            payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];

          if (!role) {
            console.warn("Role not found in token!");
          }

          localStorage.setItem("token", token);
          if (role) localStorage.setItem("role", role);

          // REDIRECT BASED ON ROLE
          if (role === "SuperAdmin") {
            this.router.navigateByUrl("/dashboard");
          }
          if (role === "Admin") {
            this.router.navigateByUrl("/dashboard");
          }
          if (role === "MainAdmin") {
            this.router.navigateByUrl("/dashboard");
          }
          else if (role === "Cashier") {
            this.router.navigateByUrl("/ORDERCREATE");
          }
          else if (role === "StoreKeeper") {
            this.router.navigateByUrl("/MainPO");
          }
          else if (role === "Waiter") {
        this.router.navigateByUrl("/ORDERCREATE");
      }
          else if (role === "Rider") {
            this.router.navigateByUrl("/my-deliveries");
          }
        }
        catch (err) {
          console.error("Invalid token format", err);
          this.toast.error("Invalid token received.");
        }
      },

      error: (err) => {
        console.error(err);
        if (err.status === 401 || err.status === 400) {
          this.toast.error("Invalid username or password");
        } else if (err.status === 0) {
          this.toast.error("Cannot reach server - check that the backend is running");
        } else {
          this.toast.error(`Login failed (${err.status}): ${err.error ?? err.message}`);
        }
      }
    });
  }

  togglePassword() {
    this.showPassword = !this.showPassword;
  }
}
