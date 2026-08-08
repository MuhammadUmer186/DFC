import { Component, OnInit, signal } from '@angular/core';
import { Category } from '../../../Models/category.model';
import { CategoryService } from '../../../Services/category.service';
import { RouterLink } from '@angular/router';
import { ToastService } from '../../../Services/toast.service';
import { environment } from '../../../../environments/environment';

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

  // 📷 Image upload
  uploadingId = signal<number | null>(null);

  constructor(private service: CategoryService, private toast: ToastService) {}

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
      .subscribe({
        next: () => {
          this.toast.success('Category added');
          this.newCategoryName.set('');
          this.load();
        },
        error: (err) => this.toast.error(err?.error || 'Failed to add category')
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
      .subscribe({
        next: () => {
          this.toast.success('Category updated');
          this.editingId.set(null);
          this.load();
        },
        error: (err) => this.toast.error(err?.error || 'Failed to update category')
      });
  }

  // ❌ Cancel edit
  cancelEdit() {
    this.editingId.set(null);
  }

  // 🗑 Delete
  delete(id: number) {
    if (!confirm('Delete category?')) return;
    this.service.delete(id).subscribe({
      next: () => {
        this.toast.success('Category deleted');
        this.load();
      },
      error: (err) => this.toast.error(err?.error || 'Failed to delete category — it may still have menu items assigned to it')
    });
  }

  // 📷 Build a full, browsable URL for a stored (relative) image path
  fullImageUrl(imageUrl?: string | null): string | null {
    if (!imageUrl) return null;
    return `${environment.apihub}${imageUrl}`;
  }

  // 📷 Upload / replace a category photo
  onFileSelected(cat: Category, event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.uploadingId.set(cat.id);

    this.service.uploadImage(cat.id, file).subscribe({
      next: () => {
        this.toast.success('Category photo updated');
        this.uploadingId.set(null);
        input.value = '';
        this.load();
      },
      error: () => {
        this.toast.error('Failed to upload photo');
        this.uploadingId.set(null);
        input.value = '';
      }
    });
  }
}
