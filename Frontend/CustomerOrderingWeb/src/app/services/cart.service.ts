import { Injectable, computed, signal } from '@angular/core';
import { CartLine } from '../models/cart.model';

const STORAGE_KEY = 'dfc-customer-cart';

@Injectable({ providedIn: 'root' })
export class CartService {
  lines = signal<CartLine[]>(this.loadFromStorage());

  totalQuantity = computed(() => this.lines().reduce((sum, l) => sum + l.quantity, 0));
  totalAmount = computed(() => this.lines().reduce((sum, l) => sum + l.price * l.quantity, 0));

  private loadFromStorage(): CartLine[] {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw ? JSON.parse(raw) : [];
    } catch {
      return [];
    }
  }

  private persist() {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(this.lines()));
  }

  add(line: Omit<CartLine, 'quantity'>, quantity = 1) {
    this.lines.update(current => {
      const existing = current.find(l => l.kind === line.kind && l.id === line.id);
      if (existing) {
        return current.map(l => l === existing ? { ...l, quantity: l.quantity + quantity } : l);
      }
      return [...current, { ...line, quantity }];
    });
    this.persist();
  }

  setQuantity(kind: 'item' | 'deal', id: number, quantity: number) {
    this.lines.update(current => {
      if (quantity <= 0) return current.filter(l => !(l.kind === kind && l.id === id));
      return current.map(l => (l.kind === kind && l.id === id) ? { ...l, quantity } : l);
    });
    this.persist();
  }

  remove(kind: 'item' | 'deal', id: number) {
    this.lines.update(current => current.filter(l => !(l.kind === kind && l.id === id)));
    this.persist();
  }

  clear() {
    this.lines.set([]);
    this.persist();
  }
}
