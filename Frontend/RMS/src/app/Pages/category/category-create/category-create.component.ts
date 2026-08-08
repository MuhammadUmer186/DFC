import { Component, OnInit } from '@angular/core';

import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CategoryService } from '../../../Services/category.service';

@Component({
  standalone: true,
  selector: 'app-category-create',
  imports: [ReactiveFormsModule],
  templateUrl: './category-create.component.html'
})
export class CategoryCreateComponent implements OnInit {

  form!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private service: CategoryService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      name: ['', Validators.required]
    });
  }

  save(): void {
    if (this.form.invalid) return;

    this.service.create(this.form.value)
      .subscribe(() => {
        this.router.navigate(['/categories']);
      });
  }
}
