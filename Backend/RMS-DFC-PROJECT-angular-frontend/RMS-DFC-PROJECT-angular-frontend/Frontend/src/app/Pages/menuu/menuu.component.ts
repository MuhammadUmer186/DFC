import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Category } from '../../Models/category.model';
import { MenuService } from '../../Services/menu.service';
import { MenuItem } from '../../Models/menu-item.model';
import { CommonModule } from '@angular/common';


@Component({
  selector: 'app-menuu',
  imports:[CommonModule,ReactiveFormsModule],
  templateUrl: './menuu.component.html',
  styleUrls: ['./menuu.component.css']
})
export class MenuuComponent implements OnInit {

  categories: Category[] = [];
  selectedCategory?: Category;

  menuForm!: FormGroup;
  editMode = false;

  constructor(
    private fb: FormBuilder,
    private menuService: MenuService
  ) {}

  ngOnInit(): void {
    this.loadMenu();

    this.menuForm = this.fb.group({
      id: [0],
      name: ['', Validators.required],
      price: ['', Validators.required],
      categoryId: ['', Validators.required]
    });
  }

  loadMenu() {
    this.menuService.getMenu().subscribe(res => {
      this.categories = res;
      this.selectedCategory = undefined;
    });
  }

  selectCategory(event: any) {
    const categoryId = +event.target.value;
    this.selectedCategory = this.categories.find(c => c.id === categoryId);
  }

  submit() {
    const item: MenuItem = this.menuForm.value;

    if (this.editMode) {
      this.menuService.updateMenu(item.id, item).subscribe(() => {
        this.resetForm();
        this.loadMenu();
      });
    } else {
      this.menuService.createMenu(item).subscribe(() => {
        this.resetForm();
        this.loadMenu();
      });
    }
  }

  edit(item: MenuItem) {
    this.editMode = true;
    this.menuForm.patchValue(item);
  }

  delete(id: number) {
    if (confirm('Delete this item?')) {
      this.menuService.deleteMenu(id).subscribe(() => {
        this.loadMenu();
      });
    }
  }

  resetForm() {
    this.editMode = false;
    this.menuForm.reset({ id: 0 });
  }
}
