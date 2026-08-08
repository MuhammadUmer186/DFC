import { Component, OnInit, signal } from '@angular/core';
import { Rider } from '../../../Models/rider.model';
import { RiderService } from '../../../Services/rider.service';
import { AuthService } from '../../../Services/auth.service';
import { ToastService } from '../../../Services/toast.service';

@Component({
  selector: 'app-rider-list',
  standalone: true,
  imports: [],
  templateUrl: './rider-list.component.html',
  styleUrl: './rider-list.component.css'
})
export class RiderListComponent implements OnInit {

  riders = signal<Rider[]>([]);

  // ➕ Add (creates the rider profile AND its login account together)
  newName = signal('');
  newPhone = signal('');
  newVehicle = signal('');
  newUsername = signal('');
  newPassword = signal('');
  adding = signal(false);

  // ✏ Edit
  editingId = signal<number | null>(null);
  editName = signal('');
  editPhone = signal('');
  editVehicle = signal('');
  editActive = signal(true);

  constructor(private service: RiderService, private authService: AuthService, private toast: ToastService) {}

  ngOnInit() {
    this.load();
  }

  load() {
    this.service.getAll().subscribe({
      next: (res) => this.riders.set(res),
      error: () => this.toast.error('Failed to load riders')
    });
  }

  add() {
    const name = this.newName().trim();
    const phone = this.newPhone().trim();
    const username = this.newUsername().trim();
    const password = this.newPassword();

    if (!name || !phone) {
      this.toast.error('Name and phone number are required');
      return;
    }
    if (!username || !password) {
      this.toast.error('Username and password are required to create the rider\'s login');
      return;
    }
    if (password.length < 5) {
      this.toast.error('Password must be at least 5 characters');
      return;
    }

    this.adding.set(true);

    this.service.create({ name, phoneNumber: phone, vehicleNumber: this.newVehicle().trim() || null })
      .subscribe({
        next: (rider) => {
          this.authService.createUser({ userName: username, password, roles: 'Rider', riderId: rider.id })
            .subscribe({
              next: () => {
                this.toast.success(`Rider "${name}" added — login "${username}" is ready`);
                this.resetAddForm();
                this.load();
              },
              error: (err) => {
                this.toast.error(
                  (err.error?.message || err.error || 'Failed to create the rider\'s login') +
                  ' — the rider profile was saved; open Edit or the Register page to create their login.'
                );
                this.resetAddForm();
                this.load();
              }
            });
        },
        error: (err) => {
          this.toast.error(err?.error || 'Failed to add rider');
          this.adding.set(false);
        }
      });
  }

  private resetAddForm() {
    this.newName.set('');
    this.newPhone.set('');
    this.newVehicle.set('');
    this.newUsername.set('');
    this.newPassword.set('');
    this.adding.set(false);
  }

  startEdit(rider: Rider) {
    this.editingId.set(rider.id);
    this.editName.set(rider.name);
    this.editPhone.set(rider.phoneNumber);
    this.editVehicle.set(rider.vehicleNumber || '');
    this.editActive.set(rider.isActive);
  }

  saveEdit(rider: Rider) {
    const name = this.editName().trim();
    const phone = this.editPhone().trim();
    if (!name || !phone) return;

    this.service.update(rider.id, {
      name,
      phoneNumber: phone,
      vehicleNumber: this.editVehicle().trim() || null,
      isActive: this.editActive()
    }).subscribe({
      next: () => {
        this.toast.success('Rider updated');
        this.editingId.set(null);
        this.load();
      },
      error: (err) => this.toast.error(err?.error || 'Failed to update rider')
    });
  }

  cancelEdit() {
    this.editingId.set(null);
  }

  delete(rider: Rider) {
    if (!confirm(`Permanently delete rider "${rider.name}"? This cannot be undone.`)) return;
    this.service.delete(rider.id).subscribe({
      next: () => {
        this.toast.success('Rider permanently deleted');
        this.load();
      },
      error: (err) => this.toast.error(err?.error || 'Failed to delete rider')
    });
  }
}
