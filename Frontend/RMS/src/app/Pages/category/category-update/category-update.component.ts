
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CategoryService } from '../../../Services/category.service';
import { Category } from '../../../Models/category.model';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './category-update.component.html'
})
export class CategoryUpdateComponent implements OnInit {

  form!: FormGroup;

  constructor(
    private route: ActivatedRoute,
    private service: CategoryService,
    private fb: FormBuilder,
    private router: Router
  ) {}

  ngOnInit(): void {

    // ✅ Initialize form AFTER fb is available
    this.form = this.fb.group({
      id: [1],
      name: ['', Validators.required]
    });

    const id = Number(this.route.snapshot.paramMap.get('id'));

    this.service.getById(id).subscribe(res => {
      this.form.patchValue(res);
    });
  }

  update(): void {
    if (this.form.invalid) return;

    const id = this.form.value.id;

    this.service.update(id, this.form.value as Category)
      .subscribe(() => {
        this.router.navigate(['/categories']);
      });
  }
}
