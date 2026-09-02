import { Component, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  MenuRecipeService, RawItemLite, RecipeItem,
  RecipeOverviewCategory, RecipeOverviewItem, KitchenAuditReport
} from '../../Services/menu-recipe';
import { ToastService } from '../../Services/toast.service';

@Component({
  selector: 'app-menu-recipe',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './menu-recipe.html',
  styleUrls: ['./menu-recipe.css']
})
export class MenuRecipeComponent {

  tab = signal<'recipes' | 'audit'>('recipes');

  // ---- Recipes tab ----
  categories = signal<RecipeOverviewCategory[]>([]);
  expanded = signal<Set<number>>(new Set());
  rawItems = signal<RawItemLite[]>([]);
  selected = signal<RecipeOverviewItem | null>(null);
  recipeItems = signal<RecipeItem[]>([]);
  loadingRecipe = signal(false);
  saving = signal(false);

  coverage = computed(() => {
    const cats = this.categories();
    const total = cats.reduce((s, c) => s + c.itemCount, 0);
    const withRecipe = cats.reduce((s, c) => s + c.itemsWithRecipe, 0);
    return { total, withRecipe, pct: total ? Math.round((withRecipe / total) * 100) : 0 };
  });

  // ---- Audit tab ----
  from = signal<string>(this.isoDaysAgo(7));
  to = signal<string>(this.isoToday());
  audit = signal<KitchenAuditReport | null>(null);
  loadingAudit = signal(false);
  openDish = signal<number | null>(null);

  constructor(private svc: MenuRecipeService, private toast: ToastService) {
    this.loadOverview();
    this.svc.getAllRawItems().subscribe({
      next: r => this.rawItems.set(r ?? []),
      error: () => this.rawItems.set([])
    });
  }

  // ================= Recipes =================

  loadOverview() {
    this.svc.getOverview().subscribe({
      next: c => this.categories.set(c ?? []),
      error: () => this.toast.error('Could not load the menu overview')
    });
  }

  toggleCat(id: number) {
    this.expanded.update(s => {
      const n = new Set(s);
      n.has(id) ? n.delete(id) : n.add(id);
      return n;
    });
  }
  isOpen = (id: number) => this.expanded().has(id);

  pick(item: RecipeOverviewItem) {
    this.selected.set(item);
    this.recipeItems.set([]);
    this.loadingRecipe.set(true);
    this.svc.getRecipeByMenuItemId(item.menuItemId).subscribe({
      next: res => {
        this.recipeItems.set((res ?? []).map(r => ({
          rawItemId: r.rawItemId, rawItemName: r.rawItemName,
          quantityRequired: r.quantityRequired, unit: r.unit
        })));
        this.loadingRecipe.set(false);
      },
      error: () => { this.loadingRecipe.set(false); this.toast.error('Could not load this recipe'); }
    });
  }

  addRow() {
    this.recipeItems.update(r => [...r, { rawItemId: 0, quantityRequired: 0, unit: '' }]);
  }
  removeRow(i: number) {
    this.recipeItems.update(r => r.filter((_, idx) => idx !== i));
  }
  unitFor(rawItemId: number): string {
    return this.rawItems().find(x => x.id === rawItemId)?.unit ?? '';
  }

  save() {
    const item = this.selected();
    if (!item) return;
    const rows = this.recipeItems().filter(r => r.rawItemId > 0 && r.quantityRequired > 0);
    if (rows.length === 0) { this.toast.error('Add at least one ingredient with a quantity'); return; }

    this.saving.set(true);
    this.svc.assignRecipe({
      menuItemId: item.menuItemId,
      recipeItems: rows.map(r => ({ rawItemId: r.rawItemId, quantityRequired: r.quantityRequired }))
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.toast.success(`Recipe saved for “${item.menuItemName}”`);
        this.bumpCount(item.menuItemId, rows.length);
      },
      error: (e) => { this.saving.set(false); this.toast.error('Save failed: ' + (e?.error?.message || e?.message || e)); }
    });
  }

  clear() {
    const item = this.selected();
    if (!item) return;
    if (!confirm(`Remove the recipe for “${item.menuItemName}”?`)) return;
    this.svc.deleteRecipe(item.menuItemId).subscribe({
      next: () => {
        this.recipeItems.set([]);
        this.toast.success('Recipe removed');
        this.bumpCount(item.menuItemId, 0);
      },
      error: () => this.toast.error('Delete failed')
    });
  }

  private bumpCount(menuItemId: number, count: number) {
    this.categories.update(cats => cats.map(c => {
      const items = c.items.map(i => i.menuItemId === menuItemId ? { ...i, ingredientCount: count } : i);
      return { ...c, items, itemsWithRecipe: items.filter(i => i.ingredientCount > 0).length };
    }));
    const sel = this.selected();
    if (sel && sel.menuItemId === menuItemId) this.selected.set({ ...sel, ingredientCount: count });
  }

  // ================= Kitchen audit =================

  runAudit() {
    this.loadingAudit.set(true);
    this.audit.set(null);
    const fromIso = new Date(this.from() + 'T00:00:00').toISOString();
    const toIso = new Date(this.to() + 'T23:59:59').toISOString();
    this.svc.getKitchenAudit(fromIso, toIso).subscribe({
      next: r => { this.audit.set(r); this.loadingAudit.set(false); },
      error: () => { this.loadingAudit.set(false); this.toast.error('Could not run the kitchen audit'); }
    });
  }

  toggleDish(id: number) {
    this.openDish.set(this.openDish() === id ? null : id);
  }

  exportCsv() {
    const a = this.audit();
    if (!a) return;
    const rows = [['Raw item', 'Unit', 'Expected (sales)', 'Actual (kitchen-out)', 'Variance']];
    a.totals.forEach(t => rows.push([t.rawItemName, t.unit,
      String(t.expectedFromSales), String(t.actualConsumed), String(t.variance)]));
    const csv = rows.map(r => r.map(c => `"${(c ?? '').toString().replace(/"/g, '""')}"`).join(',')).join('\n');
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `kitchen-audit_${this.from()}_${this.to()}.csv`;
    link.click();
    URL.revokeObjectURL(url);
  }

  private isoToday(): string { return new Date().toISOString().slice(0, 10); }
  private isoDaysAgo(n: number): string {
    const d = new Date(); d.setDate(d.getDate() - n);
    return d.toISOString().slice(0, 10);
  }
}
