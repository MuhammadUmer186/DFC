import { Component, OnInit, signal } from '@angular/core';
import { Category } from '../../../Models/category.model';
import { CategoryService } from '../../../Services/category.service';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-category-list',
  standalone: true,
  imports: [],
  templateUrl: './category-list.component.html',
  styleUrl: './category-list.component.css'
})
export class CategoryListComponent implements OnInit {

  categories = signal<Category[]>([]);

  // ➕ Add
  newCategoryName = signal('');

  // ✏ Edit
  editingId = signal<number | null>(null);
  editName = signal('');

  constructor(private service: CategoryService) {}

  ngOnInit() {
    this.load();
  }

  load() {
    this.service.getAll().subscribe(res => this.categories.set(res));
  }

  // ➕ Add category
  add() {
    const name = this.newCategoryName().trim();
    if (!name) return;

    this.service.create({ id: 0, name } as Category)
      .subscribe(() => {
        this.newCategoryName.set('');
        this.load();
      });
  }

  // ✏ Start edit
  startEdit(cat: Category) {
    this.editingId.set(cat.id);
    this.editName.set(cat.name);
  }

  // 💾 Save edit
  saveEdit(cat: Category) {
    const name = this.editName().trim();
    if (!name) return;

    this.service.update(cat.id, { ...cat, name })
      .subscribe(() => {
        this.editingId.set(null);
        this.load();
      });
  }

  // ❌ Cancel edit
  cancelEdit() {
    this.editingId.set(null);
  }

  // 🗑 Delete
  delete(id: number) {
    if (!confirm('Delete category?')) return;
    this.service.delete(id).subscribe(() => this.load());
  }
}
