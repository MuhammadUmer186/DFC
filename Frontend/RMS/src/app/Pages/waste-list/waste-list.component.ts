import { Component, signal } from '@angular/core';
import { WasteRecord, WasteService } from '../../Services/waste.service';
import { CommonModule } from '@angular/common';
import { RmsDatePipe } from '../../Shared/pipes/rms-date.pipe';

@Component({
  selector: 'app-waste-list',
  imports: [CommonModule, RmsDatePipe],
  templateUrl: './waste-list.component.html',
  styleUrls: ['./waste-list.component.css'] // Fixed typo 'styleUrl' -> 'styleUrls'
})
export class WasteListComponent {

  loading = signal(true);
  wastes = signal<WasteRecord[]>([]);

  // Signal to store expanded waste row
  expandedWasteId = signal<number | null>(null);

  constructor(private service: WasteService) {
    this.loadWasteRecords();
  }

  loadWasteRecords() {
    this.service.getAll().subscribe({
      next: res => {
        console.log(res);
        this.wastes.set(res);
        this.loading.set(false);
      },
      error: err => {
        console.log(err);
        this.loading.set(false);
      }
    });
  }

  // Toggle row expansion
  toggleWasteDetails(id: number) {
    this.expandedWasteId.set(this.expandedWasteId() === id ? null : id);
  }
}
