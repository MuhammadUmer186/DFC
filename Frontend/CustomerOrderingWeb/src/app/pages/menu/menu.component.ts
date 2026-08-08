import { Component, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MenuService } from '../../services/menu.service';
import { CartService } from '../../services/cart.service';
import { CartPanelComponent } from '../../components/cart-panel/cart-panel.component';
import { SiteSettingService } from '../../services/site-setting.service';
import { PublicMenu, PublicMenuItem, PublicDeal } from '../../models/menu.model';

const CATEGORY_ICONS: { keyword: string; icon: string }[] = [
  { keyword: 'fries', icon: '🍟' },
  { keyword: 'burger', icon: '🍔' },
  { keyword: 'shawarma', icon: '🌯' },
  { keyword: 'pizza', icon: '🍕' },
  { keyword: 'beverage', icon: '🥤' },
  { keyword: 'drink', icon: '🥤' },
  { keyword: 'side', icon: '🍗' },
  { keyword: 'extra', icon: '🍗' },
  { keyword: 'nugget', icon: '🍗' },
  { keyword: 'chicken', icon: '🍗' },
  { keyword: 'dessert', icon: '🍰' },
];

const FAVORITES_KEY = 'dfc-customer-favorites';

@Component({
  selector: 'app-menu',
  standalone: true,
  imports: [CommonModule, CartPanelComponent],
  templateUrl: './menu.component.html',
  styleUrl: './menu.component.css'
})
export class MenuComponent {
  menu = signal<PublicMenu | null>(null);
  loading = signal<boolean>(true);
  error = signal<boolean>(false);
  activeCategoryId = signal<number | null>(null);
  searchTerm = signal<string>('');

  favorites = signal<Set<number>>(this.loadFavorites());

  visibleItems = computed(() => {
    const m = this.menu();
    if (!m) return [];

    const term = this.searchTerm().trim().toLowerCase();
    if (term) {
      return m.categories.flatMap(c => c.items).filter(i => i.name.toLowerCase().includes(term));
    }

    const catId = this.activeCategoryId();
    const cat = catId == null ? m.categories[0] : m.categories.find(c => c.id === catId);
    return cat?.items ?? [];
  });

  addedFlash = signal<Set<number>>(new Set());

  constructor(
    public menuService: MenuService,
    public cart: CartService,
    public siteSettings: SiteSettingService
  ) {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.error.set(false);
    this.menuService.getMenu().subscribe({
      next: (menu) => {
        this.menu.set(menu);
        if (menu.categories.length > 0) this.activeCategoryId.set(menu.categories[0].id);
        this.loading.set(false);
      },
      error: () => {
        this.error.set(true);
        this.loading.set(false);
      }
    });
  }

  selectCategory(id: number) {
    this.searchTerm.set('');
    this.activeCategoryId.set(id);
  }

  categoryIcon(name: string): string {
    const lower = name.toLowerCase();
    const match = CATEGORY_ICONS.find(c => lower.includes(c.keyword));
    return match?.icon ?? '🍽️';
  }

  private loadFavorites(): Set<number> {
    try {
      const raw = localStorage.getItem(FAVORITES_KEY);
      return raw ? new Set(JSON.parse(raw)) : new Set();
    } catch {
      return new Set();
    }
  }

  toggleFavorite(itemId: number, event: Event) {
    event.stopPropagation();
    this.favorites.update(set => {
      const next = new Set(set);
      if (next.has(itemId)) next.delete(itemId); else next.add(itemId);
      localStorage.setItem(FAVORITES_KEY, JSON.stringify(Array.from(next)));
      return next;
    });
  }

  isFavorite(itemId: number): boolean {
    return this.favorites().has(itemId);
  }

  addItem(item: PublicMenuItem) {
    this.cart.add({
      kind: 'item',
      id: item.id,
      name: item.name,
      price: item.price,
      imageUrl: this.menuService.fullImageUrl(item.imageUrl)
    });
    this.addedFlash.update(set => new Set(set).add(item.id));
    setTimeout(() => {
      this.addedFlash.update(set => {
        const next = new Set(set);
        next.delete(item.id);
        return next;
      });
    }, 700);
  }

  addDeal(deal: PublicDeal) {
    this.cart.add({
      kind: 'deal',
      id: deal.id,
      name: deal.dealName,
      price: deal.price,
      imageUrl: this.menuService.fullImageUrl(deal.imageUrl)
    });
  }

  itemQuantity(itemId: number): number {
    return this.cart.lines().find(l => l.kind === 'item' && l.id === itemId)?.quantity ?? 0;
  }

  scrollToItems() {
    document.getElementById('menu-items')?.scrollIntoView({ behavior: 'smooth' });
  }
}
