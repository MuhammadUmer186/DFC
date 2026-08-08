import { Component, OnInit, signal } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';

import { RawItemService } from '../../Services/rawitem.service';
import { AuthService } from '../../Services/auth.service';
import { ToastService } from '../../Services/toast.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-waste-management',
  standalone: true,
  imports: [FormsModule, ReactiveFormsModule],
  templateUrl: './wastemanagement.component.html',
  styleUrls: ['./wastemanagement.component.css']
})
export class WasteManagementComponent{
  private baseUrll = environment.apiBaseUrl;
  wasteForm!: FormGroup;
  rawItems=signal<any[]>([]);
  submitting = false;

  constructor(private fb: FormBuilder,private router:Router, private http: HttpClient,private rawserv:RawItemService,private authservice:AuthService, private toast: ToastService) {


    this.wasteForm = this.fb.group({
      reason: ['', Validators.required],
      items: this.fb.array([])
    });

    this.addWasteItem(); // Add 1 row initially
    this.loadRawItems();
  }

  

  get items(): FormArray {
    return this.wasteForm.get('items') as FormArray;
  }

  addWasteItem() {
    this.items.push(
      this.fb.group({
        rawItemId: ['', Validators.required],
        quantity: ['', [Validators.required, Validators.min(1)]]
      })
    );
  }

  removeItem(index: number) {
    this.items.removeAt(index);
  }

  loadRawItems() {
    this.rawserv.getAll().subscribe(res => {
      this.rawItems.set(res);
  })}

  submitWaste() {
    if (this.wasteForm.invalid) {
      this.toast.warn("Please fill required fields");
      return;
    }

    this.submitting = true;
    const reqHeader=new HttpHeaders({'Authorization':'Bearer ' + this.authservice.gettoken()})
    this.http.post(this.baseUrll + '/Waste', this.wasteForm.value,{headers:reqHeader})
      .subscribe({
        next: () => {
          this.toast.success("Waste Recorded Successfully");
          this.router.navigate(['/WM-list']);
          this.submitting = false;
          this.wasteForm.reset();
          this.items.clear();
          this.addWasteItem();
        },
        error: (err) => {
          this.submitting = false;
          this.toast.error(err.error || "Failed to submit");
        }
      });
  }

}
