import { Component, OnInit, signal } from '@angular/core';

import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { MenuService } from '../../../Services/menu.service';
import { CategoryService } from '../../../Services/category.service';
import { Category } from '../../../Models/category.model';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './menu-update.component.html'
})
export class MenuUpdateComponent implements OnInit {

  categories = signal<Category[]>([]); 
  id!: number;

  form: any;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private menuService: MenuService,
    private categoryService: CategoryService,
    private router: Router
  ) {
    this.form = this.fb.group({
      name: ['', Validators.required],
      price: [0, Validators.required],
      categoryId: [0, Validators.required]
    });
  }

  ngOnInit() {
    this.id = Number(this.route.snapshot.paramMap.get('id'));

    // Load categories
    this.categoryService.getAll().subscribe({
      next: (res) => this.categories.set(res),
      error: () => console.error('Failed to load categories')
    });

    // Load ALL menus then pick needed record
    this.menuService.getMenu()
      .subscribe((menus: any[]) => {
        const record = menus.find(m => m.id == this.id);

        if (record) {
          this.form.patchValue(record);
        }
      });
  }

  update() {
    if (this.form.invalid) return;

    this.menuService.updateMenu(this.id, this.form.value)
      .subscribe(() => this.router.navigate(['/menu']));
  }
}
