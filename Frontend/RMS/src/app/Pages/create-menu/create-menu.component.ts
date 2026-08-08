import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';

import { MenuService } from '../../Services/menu.service';
import { Category } from '../../Models/category.model';
import { MenuItem } from '../../Models/menu-item.model';
import { ToastService } from '../../Services/toast.service';

@Component({
  selector: 'app-create-menu',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './create-menu.component.html',
  styleUrls: ['./create-menu.component.css']
})
export class CreateMenuComponent implements OnInit {

  categories: Category[] = [];
  form!: FormGroup;

  constructor(private fb: FormBuilder, private menuService: MenuService,private toast: ToastService,) {}

  ngOnInit() {
    this.menuService.getMenu().subscribe(res => this.categories = res);

    this.form = this.fb.group({
      name: ['', Validators.required],
      price: ['', Validators.required],
      categoryId: ['', Validators.required]
    });
  }

  submit() {
    if (this.form.invalid) return;

    this.menuService.createMenu(this.form.value).subscribe(() => {
      this.toast.success('Menu item created');
      this.form.reset();
    });
  }
}
