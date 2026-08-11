import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AiInventoryService, InventoryRecommendation } from '../../../Services/ai-inventory.service';
import { ToastService } from '../../../Services/toast.service';

@Component({
  selector: 'app-ai-inventory',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ai-inventory.component.html',
  styleUrl: './ai-inventory.component.css'
})
export class AiInventoryComponent {
  recommendations = signal<InventoryRecommendation[]>([]);
  loading = signal(false);
  recalculating = signal(false);
  busyIds = signal<Set<number>>(new Set());
  statusFilter = signal<'Pending' | 'Approved' | 'Rejected' | 'Modified' | 'All'>('Pending');
  modifyDrafts = new Map<number, number>();

  constructor(private service: AiInventoryService, private toast: ToastService) {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.service.getAll(this.statusFilter()).subscribe({
      next: (list) => { this.recommendations.set(list); this.loading.set(false); },
      error: () => { this.toast.error('Failed to load inventory recommendations'); this.loading.set(false); }
    });
  }

  setStatusFilter(status: 'Pending' | 'Approved' | 'Rejected' | 'Modified' | 'All') {
    this.statusFilter.set(status);
    this.load();
  }

  recalculate() {
    this.recalculating.set(true);
    this.service.recalculate().subscribe({
      next: (list) => {
        this.recalculating.set(false);
        this.toast.success(`Generated ${list.length} recommendation(s)`);
        this.load();
      },
      error: (err) => {
        this.recalculating.set(false);
        this.toast.error(err?.error?.message || 'Failed to recalculate recommendations');
      }
    });
  }

  isBusy(id: number): boolean { return this.busyIds().has(id); }
  private setBusy(id: number, busy: boolean) {
    this.busyIds.update(set => { const next = new Set(set); if (busy) next.add(id); else next.delete(id); return next; });
  }

  getModifyDraft(id: number, fallback: number): number {
    return this.modifyDrafts.has(id) ? this.modifyDrafts.get(id)! : fallback;
  }
  setModifyDraft(id: number, value: string) {
    this.modifyDrafts.set(id, Number(value));
  }

  approve(rec: InventoryRecommendation) { this.decide(rec, 'Approved'); }
  reject(rec: InventoryRecommendation) { this.decide(rec, 'Rejected'); }
  modify(rec: InventoryRecommendation) {
    const qty = this.getModifyDraft(rec.id, rec.suggestedReorderQuantity);
    if (qty < 0) { this.toast.error('Quantity cannot be negative'); return; }
    this.decide(rec, 'Modified', qty);
  }

  private decide(rec: InventoryRecommendation, decision: 'Approved' | 'Rejected' | 'Modified', modifiedQuantity?: number) {
    this.setBusy(rec.id, true);
    this.service.decide(rec.id, decision, modifiedQuantity).subscribe({
      next: () => {
        this.setBusy(rec.id, false);
        this.toast.success(`Recommendation ${decision.toLowerCase()}`);
        this.recommendations.update(list => list.filter(r => r.id !== rec.id));
      },
      error: (err) => {
        this.setBusy(rec.id, false);
        this.toast.error(err?.error?.message || 'Failed to record decision');
      }
    });
  }

  typeBadgeClass(type: string): string {
    switch (type) {
      case 'LowStock': return 'dfc-badge-danger';
      case 'Reorder': return 'dfc-badge-amber';
      case 'ExcessStock': return 'dfc-badge-info';
      case 'ExpiryRisk': return 'dfc-badge-danger';
      case 'WasteReduction': return 'dfc-badge-navy';
      default: return 'dfc-badge-info';
    }
  }
}
