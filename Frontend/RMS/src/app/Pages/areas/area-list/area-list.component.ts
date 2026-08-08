import { Component, OnInit, signal } from '@angular/core';
import { Area } from '../../../Models/area.model';
import { AreaService } from '../../../Services/area.service';
import { ToastService } from '../../../Services/toast.service';

@Component({
  selector: 'app-area-list',
  standalone: true,
  imports: [],
  templateUrl: './area-list.component.html',
  styleUrl: './area-list.component.css'
})
export class AreaListComponent implements OnInit {

  areas = signal<Area[]>([]);

  // ➕ Add
  newName = signal('');
  newFee = signal<number | null>(null);
  adding = signal(false);

  // ✏ Edit
  editingId = signal<number | null>(null);
  editName = signal('');
  editFee = signal<number | null>(null);
  editActive = signal(true);

  constructor(private service: AreaService, private toast: ToastService) {}

  ngOnInit() {
    this.load();
  }

  setNewFee(value: string) {
    this.newFee.set(value ? Number(value) : null);
  }

  setEditFee(value: string) {
    this.editFee.set(value ? Number(value) : null);
  }

  load() {
    this.service.getAll().subscribe({
      next: (res) => this.areas.set(res),
      error: () => this.toast.error('Failed to load areas')
    });
  }

  add() {
    const name = this.newName().trim();
    const fee = this.newFee();

    if (!name) {
      this.toast.error('Area name is required');
      return;
    }
    if (fee == null || fee < 0) {
      this.toast.error('Enter a valid delivery fee');
      return;
    }

    this.adding.set(true);

    this.service.create({ name, deliveryFee: fee }).subscribe({
      next: () => {
        this.toast.success(`Area "${name}" added`);
        this.newName.set('');
        this.newFee.set(null);
        this.adding.set(false);
        this.load();
      },
      error: (err) => {
        this.toast.error(err?.error || 'Failed to add area');
        this.adding.set(false);
      }
    });
  }

  startEdit(area: Area) {
    this.editingId.set(area.id);
    this.editName.set(area.name);
    this.editFee.set(area.deliveryFee);
    this.editActive.set(area.isActive);
  }

  saveEdit(area: Area) {
    const name = this.editName().trim();
    const fee = this.editFee();
    if (!name || fee == null || fee < 0) return;

    this.service.update(area.id, {
      name,
      deliveryFee: fee,
      isActive: this.editActive()
    }).subscribe({
      next: () => {
        this.toast.success('Area updated');
        this.editingId.set(null);
        this.load();
      },
      error: (err) => this.toast.error(err?.error || 'Failed to update area')
    });
  }

  cancelEdit() {
    this.editingId.set(null);
  }

  delete(area: Area) {
    if (!confirm(`Permanently delete area "${area.name}"? This cannot be undone.`)) return;
    this.service.delete(area.id).subscribe({
      next: () => {
        this.toast.success('Area permanently deleted');
        this.load();
      },
      error: (err) => this.toast.error(err?.error || 'Failed to delete area')
    });
  }
}
