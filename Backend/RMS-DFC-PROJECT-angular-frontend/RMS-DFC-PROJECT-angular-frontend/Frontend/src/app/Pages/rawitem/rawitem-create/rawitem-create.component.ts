import { Component } from '@angular/core';

import { ReactiveFormsModule, FormControl, Validators, FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { RawItem, RawItemService } from '../../../Services/rawitem.service';
import { ToastService } from '../../../Services/toast.service';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule],
  templateUrl: './rawitem-create.component.html',
  styleUrls: ['./rawitem-create.component.css']
})
export class RawItemCreateComponent {

  name = new FormControl('', Validators.required);
  unit = new FormControl('', Validators.required);

  constructor(private service: RawItemService, private router: Router,private toast: ToastService) {}

  save() {
    if (this.name.invalid || this.unit.invalid) return;

    const data:any = {
      name: this.name.value || '',
      unit: this.unit.value || ''
    };

    this.service.create(data).subscribe({
  next: () => {
    this.toast.success('Raw Item added successfully!');  // ✅ Success alert
    this.router.navigate(['/rawitem-create']);
  },
  error: err => {
    console.error(err);
    this.toast.error('Failed to add Raw Item. Please try again.');  // ❌ Error alert
  }
});

  }
}
