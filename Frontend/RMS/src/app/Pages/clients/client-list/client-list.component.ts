import { Component, OnInit, signal, computed } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Client } from '../../../Models/client.model';
import { ClientService } from '../../../Services/client.service';
import { ToastService } from '../../../Services/toast.service';

@Component({
  selector: 'app-client-list',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './client-list.component.html',
  styleUrl: './client-list.component.css'
})
export class ClientListComponent implements OnInit {

  clients = signal<Client[]>([]);
  search = signal('');

  filteredClients = computed(() => {
    const term = this.search().trim().toLowerCase();
    if (!term) return this.clients();
    return this.clients().filter(c =>
      (c.name || '').toLowerCase().includes(term) ||
      c.phoneNumber.toLowerCase().includes(term) ||
      (c.address || '').toLowerCase().includes(term)
    );
  });

  constructor(private service: ClientService, private toast: ToastService) {}

  ngOnInit() {
    this.load();
  }

  load() {
    this.service.getAll().subscribe({
      next: (res) => this.clients.set(res),
      error: () => this.toast.error('Failed to load clients')
    });
  }
}
