import { Component, OnInit } from '@angular/core';

import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { MenuService } from '../../Services/menu.service';
import { Category } from '../../Models/category.model';
import { MenuItem } from '../../Models/menu-item.model';
import { ToastService } from '../../Services/toast.service';

@Component({
  selector: 'app-update-menu',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './update-menu.component.html',
  styleUrls: ['./update-menu.component.css']
})
export class UpdateMenuComponent implements OnInit {

  categories: Category[] = [];
  selectedCategory?: Category;
  selectedItem?: MenuItem;

  form!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private menuService: MenuService,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      categoryId: ['', Validators.required],
      itemId: ['', Validators.required],
      id: [0],
      name: ['', Validators.required],
      price: ['', Validators.required]
    });

    this.loadMenu();

    // 🔹 When category changes
    this.form.get('categoryId')!.valueChanges.subscribe(categoryId => {
      this.selectedCategory = this.categories.find(c => c.id === +categoryId);
      this.selectedItem = undefined;

      this.form.patchValue({
        itemId: '',
        id: 0,
        name: '',
        price: ''
      });
    });

    // 🔹 When item changes
    this.form.get('itemId')!.valueChanges.subscribe(itemId => {
      if (!this.selectedCategory) return;

      this.selectedItem = this.selectedCategory.items.find(
        i => i.id === +itemId
      );

      if (this.selectedItem) {
        this.form.patchValue({
          id: this.selectedItem.id,
          name: this.selectedItem.name,
          price: this.selectedItem.price
        });
      }
    });
  }

  loadMenu() {
    this.menuService.getMenu().subscribe(res => {
      this.categories = res;
    });
  }

  update() {
    if (this.form.invalid) return;

    const item: MenuItem = {
      id: this.form.value.id,
      name: this.form.value.name,
      price: this.form.value.price,
      categoryId: this.form.value.categoryId
    };

    this.menuService.updateMenu(item.id, item).subscribe(() => {
      this.toast.success('Menu item updated');
      this.loadMenu();
      this.form.reset();
      this.selectedItem = undefined;
    });
  }
}
