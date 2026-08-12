import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MenuService } from '../../Services/menu.service';
import { CategoryService } from '../../Services/category.service';
import { ToastService } from '../../Services/toast.service';
import { Category } from '../../Models/category.model';
import { MenuItem } from '../../Models/menu-item.model';
import { environment } from '../../../environments/environment';
import { RmsCurrencyPipe } from '../../Shared/pipes/currency-symbol.pipe';

@Component({
  selector: 'app-my-menu',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, RmsCurrencyPipe],
  templateUrl: './my-menu.component.html',
  styleUrls: ['./my-menu.component.css']
})
export class MyMenuComponent implements OnInit {

  loading = signal(false);
  categories = signal<Category[]>([]);

  // Filters
  searchTerm = signal('');
  activeCategoryId = signal<number | 'all'>('all');

  // Add / Edit form state
  showForm = signal(false);
  editingItem = signal<MenuItem | null>(null);
  formName = signal('');
  formPrice = signal<number | null>(null);
  formCategoryId = signal<number | null>(null);
  formDescription = signal('');
  formImageFile: File | null = null;
  formImagePreview = signal<string | null>(null);
  saving = signal(false);

  constructor(
    private menuService: MenuService,
    private categoryService: CategoryService,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.menuService.getMenu().subscribe({
      next: (res) => {
        this.categories.set(res ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.toast.error('Failed to load menu');
        this.loading.set(false);
      }
    });
  }

  // ================= FILTERING =================
  visibleCategories = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const activeId = this.activeCategoryId();

    return this.categories()
      .filter(c => activeId === 'all' || c.id === activeId)
      .map(c => ({
        ...c,
        items: (c.items || []).filter(i => !term || i.name.toLowerCase().includes(term))
      }))
      .filter(c => !term || c.items.length > 0);
  });

  totalItemCount = computed(() =>
    this.categories().reduce((sum, c) => sum + (c.items?.length || 0), 0)
  );

  // ================= IMAGE HELPERS =================
  fullImageUrl(imageUrl?: string | null): string | null {
    if (!imageUrl) return null;
    return `${environment.apihub}${imageUrl}`;
  }

  // ================= ADD / EDIT FORM =================
  openAddForm() {
    this.editingItem.set(null);
    this.formName.set('');
    this.formPrice.set(null);
    this.formCategoryId.set(this.categories()[0]?.id ?? null);
    this.formDescription.set('');
    this.formImageFile = null;
    this.formImagePreview.set(null);
    this.showForm.set(true);
  }

  openEditForm(item: MenuItem) {
    this.editingItem.set(item);
    this.formName.set(item.name);
    this.formPrice.set(item.price);
    this.formCategoryId.set(item.categoryId);
    this.formDescription.set(item.description ?? '');
    this.formImageFile = null;
    this.formImagePreview.set(this.fullImageUrl(item.imageUrl) ?? null);
    this.showForm.set(true);
  }

  closeForm() {
    this.showForm.set(false);
    this.editingItem.set(null);
  }

  onImageSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.formImageFile = file;
    this.formImagePreview.set(URL.createObjectURL(file));
  }

  save() {
    const name = this.formName().trim();
    const price = this.formPrice();
    const categoryId = this.formCategoryId();

    if (!name || price === null || price < 0 || !categoryId) {
      this.toast.warn('Please fill in name, price and category');
      return;
    }

    this.saving.set(true);
    const editing = this.editingItem();
    const payload: MenuItem = { id: editing?.id ?? 0, name, price, categoryId, description: this.formDescription().trim() || null };

    const afterSave = (id: number) => {
      if (this.formImageFile) {
        this.menuService.uploadImage(id, this.formImageFile).subscribe({
          next: () => this.finishSave(editing ? 'Item updated' : 'Item created'),
          error: () => {
            // Text fields already saved; only the photo failed.
            this.toast.error('Item saved, but the photo failed to upload');
            this.saving.set(false);
            this.closeForm();
            this.load();
          }
        });
      } else {
        this.finishSave(editing ? 'Item updated' : 'Item created');
      }
    };

    if (editing) {
      this.menuService.updateMenu(editing.id, payload).subscribe({
        next: () => afterSave(editing.id),
        error: (err) => this.handleSaveError(err)
      });
    } else {
      this.menuService.createMenu(payload).subscribe({
        next: (res) => afterSave(res.id),
        error: (err) => this.handleSaveError(err)
      });
    }
  }

  private finishSave(message: string) {
    this.toast.success(message);
    this.saving.set(false);
    this.closeForm();
    this.load();
  }

  private handleSaveError(err: any) {
    this.toast.error(err?.error || 'Failed to save item');
    this.saving.set(false);
  }

  // ================= DELETE =================
  delete(item: MenuItem) {
    if (!confirm(`Delete "${item.name}"?`)) return;

    this.menuService.deleteMenu(item.id).subscribe({
      next: () => {
        this.toast.success('Item deleted');
        this.load();
      },
      error: (err) => this.toast.error(err?.error || 'Failed to delete item')
    });
  }
}
