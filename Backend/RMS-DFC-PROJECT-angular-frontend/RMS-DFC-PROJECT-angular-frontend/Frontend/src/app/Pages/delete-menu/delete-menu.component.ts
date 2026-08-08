import { Component, OnInit } from '@angular/core';

import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';

import { MenuService } from '../../Services/menu.service';
import { Category } from '../../Models/category.model';
import { MenuItem } from '../../Models/menu-item.model';

@Component({
  selector: 'app-delete-menu',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './delete-menu.component.html',
  styleUrls: ['./delete-menu.component.css']
})
export class DeleteMenuComponent implements OnInit {

  categories: Category[] = [];
  selectedCategory?: Category;
  selectedItem?: MenuItem;

  form!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private menuService: MenuService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      categoryId: ['', Validators.required],
      itemId: ['', Validators.required]
    });

    this.loadMenu();

    // When category changes
    this.form.get('categoryId')!.valueChanges.subscribe(categoryId => {
      this.selectedCategory = this.categories.find(c => c.id === +categoryId);
      this.selectedItem = undefined;
      this.form.patchValue({ itemId: '' });
    });

    // When item changes
    this.form.get('itemId')!.valueChanges.subscribe(itemId => {
      if (!this.selectedCategory) return;
      this.selectedItem = this.selectedCategory.items.find(i => i.id === +itemId);
    });
  }

  loadMenu() {
    this.menuService.getMenu().subscribe(res => {
      this.categories = res;
    });
  }

  delete() {
    if (!this.selectedItem) return;

    if (!confirm(`Delete "${this.selectedItem.name}" ?`)) return;

    this.menuService.deleteMenu(this.selectedItem.id).subscribe(() => {
      alert('Menu item deleted successfully');
      this.loadMenu();
      this.form.reset();
      this.selectedItem = undefined;
    });
  }
}
