import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DealsService } from '../../Services/deals.service';
import { MenuService } from '../../Services/menu.service';
import { ToastService } from '../../Services/toast.service';
import { Deal, DealMenuItem } from '../../Models/deal.model';
import { Category } from '../../Models/category.model';
import { MenuItem } from '../../Models/menu-item.model';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-deals',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './deals.component.html',
  styleUrls: ['./deals.component.css']
})
export class DealsComponent implements OnInit {

  loading = signal(false);
  deals = signal<Deal[]>([]);
  categories = signal<Category[]>([]);
  searchTerm = signal('');

  allMenuItems = computed<MenuItem[]>(() =>
    this.categories().flatMap(c => c.items || [])
  );

  visibleDeals = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    if (!term) return this.deals();
    return this.deals().filter(d => d.dealName.toLowerCase().includes(term));
  });

  // Add / Edit form state
  showForm = signal(false);
  editingDeal = signal<Deal | null>(null);
  formName = signal('');
  formDiscountAmount = signal<number>(0);
  formItems = signal<DealMenuItem[]>([]);
  formImageFile: File | null = null;
  formImagePreview = signal<string | null>(null);
  saving = signal(false);

  pickerMenuItemId = signal<number | null>(null);
  pickerQuantity = signal<number>(1);

  comboValue = computed(() => this.formItems().reduce((sum, i) => sum + i.price * i.quantity, 0));
  finalPricePreview = computed(() => Math.max(0, this.comboValue() - (this.formDiscountAmount() || 0)));

  constructor(
    private dealsService: DealsService,
    private menuService: MenuService,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.dealsService.getAll().subscribe({
      next: (res) => {
        this.deals.set(res ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.toast.error('Failed to load deals');
        this.loading.set(false);
      }
    });
    this.menuService.getMenu().subscribe({
      next: (res) => this.categories.set(res ?? []),
      error: () => {}
    });
  }

  fullImageUrl(imageUrl?: string | null): string | null {
    if (!imageUrl) return null;
    return `${environment.apihub}${imageUrl}`;
  }

  // ================= ADD / EDIT FORM =================
  openAddForm() {
    this.editingDeal.set(null);
    this.formName.set('');
    this.formDiscountAmount.set(0);
    this.formItems.set([]);
    this.formImageFile = null;
    this.formImagePreview.set(null);
    this.pickerMenuItemId.set(this.allMenuItems()[0]?.id ?? null);
    this.pickerQuantity.set(1);
    this.showForm.set(true);
  }

  openEditForm(deal: Deal) {
    this.editingDeal.set(deal);
    this.formName.set(deal.dealName);
    this.formDiscountAmount.set(deal.discountAmount);
    this.formItems.set(deal.items.map(i => ({ ...i })));
    this.formImageFile = null;
    this.formImagePreview.set(this.fullImageUrl(deal.imageUrl) ?? null);
    this.pickerMenuItemId.set(this.allMenuItems()[0]?.id ?? null);
    this.pickerQuantity.set(1);
    this.showForm.set(true);
  }

  closeForm() {
    this.showForm.set(false);
    this.editingDeal.set(null);
  }

  onImageSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.formImageFile = file;
    this.formImagePreview.set(URL.createObjectURL(file));
  }

  // ================= COMBO ITEM PICKER =================
  addItemToCombo() {
    const menuItemId = this.pickerMenuItemId();
    const quantity = this.pickerQuantity();
    if (!menuItemId || quantity < 1) return;

    const menuItem = this.allMenuItems().find(i => i.id === menuItemId);
    if (!menuItem) return;

    this.formItems.update(items => {
      const existing = items.find(i => i.menuItemId === menuItemId);
      if (existing) {
        return items.map(i => i.menuItemId === menuItemId ? { ...i, quantity: i.quantity + quantity } : i);
      }
      return [...items, { menuItemId, name: menuItem.name, price: menuItem.price, quantity }];
    });

    this.pickerQuantity.set(1);
  }

  removeItemFromCombo(menuItemId: number) {
    this.formItems.update(items => items.filter(i => i.menuItemId !== menuItemId));
  }

  // ================= SAVE =================
  save() {
    const name = this.formName().trim();
    const items = this.formItems();

    if (!name) {
      this.toast.warn('Please enter a deal name');
      return;
    }
    if (items.length === 0) {
      this.toast.warn('Add at least one item to the combo');
      return;
    }

    this.saving.set(true);
    const editing = this.editingDeal();
    const payload = {
      dealName: name,
      discountAmount: this.formDiscountAmount() || 0,
      items: items.map(i => ({ menuItemId: i.menuItemId, quantity: i.quantity }))
    };

    const afterSave = (id: number) => {
      if (this.formImageFile) {
        this.dealsService.uploadImage(id, this.formImageFile).subscribe({
          next: () => this.finishSave(editing ? 'Deal updated' : 'Deal created'),
          error: () => {
            this.toast.error('Deal saved, but the photo failed to upload');
            this.saving.set(false);
            this.closeForm();
            this.load();
          }
        });
      } else {
        this.finishSave(editing ? 'Deal updated' : 'Deal created');
      }
    };

    if (editing) {
      this.dealsService.update(editing.id, payload).subscribe({
        next: () => afterSave(editing.id),
        error: (err) => this.handleSaveError(err)
      });
    } else {
      this.dealsService.create(payload).subscribe({
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
    this.toast.error(err?.error || 'Failed to save deal');
    this.saving.set(false);
  }

  // ================= DELETE / TOGGLE =================
  delete(deal: Deal) {
    if (!confirm(`Delete "${deal.dealName}"?`)) return;

    this.dealsService.delete(deal.id).subscribe({
      next: () => {
        this.toast.success('Deal deleted');
        this.load();
      },
      error: (err) => this.toast.error(err?.error || 'Failed to delete deal')
    });
  }

  toggleActive(deal: Deal) {
    this.dealsService.toggleActive(deal.id).subscribe({
      next: (updated) => {
        this.toast.success(updated.isActive ? 'Deal activated' : 'Deal deactivated');
        this.deals.update(list => list.map(d => d.id === deal.id ? updated : d));
      },
      error: (err) => this.toast.error(err?.error || 'Failed to update deal')
    });
  }
}
