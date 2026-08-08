import { Component, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';

import { MenuService } from '../../../Services/menu.service';
import { CategoryService } from '../../../Services/category.service';
import { Category } from '../../../Models/category.model';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './menu-create.component.html',
  styleUrls: ['./menu-create.component.css']
})
export class MenuCreateComponent implements OnInit {

  categories = signal<Category[]>([]);   // ✅ SIGNAL
  form!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private menuService: MenuService,
    private categoryService: CategoryService,
    private router: Router
  ) {
    this.form = this.fb.group({
      name: ['', Validators.required],
      price: [0, [Validators.required, Validators.min(1)]],
      categoryId: ['', Validators.required]
    });
  }

  ngOnInit() {
    this.categoryService.getAll().subscribe({
      next: (res) => this.categories.set(res),
      error: () => console.error('Failed to load categories')
    });
  }

  save() {
    if (this.form.invalid) return;

    this.menuService.createMenu(this.form.value)
      .subscribe(() => this.router.navigate(['/menu']));
  }
}
