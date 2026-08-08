import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { VendorService, Vendor } from '../../../Services/vendor.service';
import { ToastService } from '../../../Services/toast.service';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './vendor-list.component.html',
  styleUrls: ['./vendor-list.component.css']
})
export class VendorListComponent implements OnInit {

  loading = signal(false);
  vendors = signal<Vendor[]>([]);
  searchTerm = signal('');

  visibleVendors = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    if (!term) return this.vendors();
    return this.vendors().filter(v =>
      v.name?.toLowerCase().includes(term) ||
      v.phone?.toLowerCase().includes(term) ||
      v.address?.toLowerCase().includes(term)
    );
  });

  // Add / Edit form state
  showForm = signal(false);
  editingVendor = signal<Vendor | null>(null);
  formName = signal('');
  formPhone = signal('');
  formAddress = signal('');
  saving = signal(false);

  constructor(private service: VendorService, private toast: ToastService) {}

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.service.getAll().subscribe({
      next: (res) => {
        this.vendors.set(res ?? []);
        this.loading.set(false);
      },
      error: () => {
        this.toast.error('Failed to load vendors');
        this.loading.set(false);
      }
    });
  }

  openAddForm() {
    this.editingVendor.set(null);
    this.formName.set('');
    this.formPhone.set('');
    this.formAddress.set('');
    this.showForm.set(true);
  }

  openEditForm(vendor: Vendor) {
    this.editingVendor.set(vendor);
    this.formName.set(vendor.name);
    this.formPhone.set(vendor.phone);
    this.formAddress.set(vendor.address);
    this.showForm.set(true);
  }

  closeForm() {
    this.showForm.set(false);
    this.editingVendor.set(null);
  }

  save() {
    const name = this.formName().trim();
    if (!name) {
      this.toast.warn('Vendor name is required');
      return;
    }

    this.saving.set(true);
    const editing = this.editingVendor();
    const payload: Vendor = {
      id: editing?.id ?? 0,
      name,
      phone: this.formPhone().trim(),
      address: this.formAddress().trim()
    };

    const onSuccess = (message: string) => {
      this.toast.success(message);
      this.saving.set(false);
      this.closeForm();
      this.load();
    };

    const onError = (err: any) => {
      this.toast.error(err?.error || 'Failed to save vendor');
      this.saving.set(false);
    };

    if (editing) {
      this.service.update(editing.id, payload).subscribe({
        next: () => onSuccess('Vendor updated'),
        error: onError
      });
    } else {
      this.service.create(payload).subscribe({
        next: () => onSuccess('Vendor created'),
        error: onError
      });
    }
  }

  delete(vendor: Vendor) {
    if (!confirm(`Delete "${vendor.name}"?`)) return;

    this.service.delete(vendor.id).subscribe({
      next: () => {
        this.toast.success('Vendor deleted');
        this.load();
      },
      error: (err) => this.toast.error(err?.error || 'Failed to delete vendor — it may have purchase orders, stock, or payment history')
    });
  }
}
