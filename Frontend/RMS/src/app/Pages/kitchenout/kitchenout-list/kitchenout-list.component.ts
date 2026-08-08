import { Component, signal } from '@angular/core';
import { KitchenOut, KitchenOutService } from '../../../Services/kitchenout.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-kitchen-out-list',
  imports: [CommonModule],
  templateUrl: './kitchenout-list.component.html',
  styleUrls: ['./kitchenout-list.component.css']
})
export class KitchenOutListComponent {

  loading = signal(true);
  kitchenOuts = signal<KitchenOut[]>([]);

  // Signal to store which kitchen out row is expanded
  expandedKitchenId = signal<number | null>(null);

  constructor(private service: KitchenOutService) {
    this.loadKitchenOuts();
  }

  loadKitchenOuts() {
    this.service.getAll().subscribe({
      next: res => {
        this.kitchenOuts.set(res);
        this.loading.set(false);
      },
      error: err => {
        console.log(err);
        this.loading.set(false);
      }
    });
  }

  // Toggle expanded row
  toggleKitchenDetails(id: number) {
    this.expandedKitchenId.set(this.expandedKitchenId() === id ? null : id);
  }
}
