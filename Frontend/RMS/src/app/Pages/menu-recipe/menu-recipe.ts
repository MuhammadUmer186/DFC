import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MenuRecipeService, RecipeItem, MenuRecipeResponse, AssignRecipeDto } from '../../Services/menu-recipe';

@Component({
  selector: 'app-menu-recipe',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './menu-recipe.html',
  styleUrls: ['./menu-recipe.css']
})
export class MenuRecipeComponent {

  // ⭐ Signals
  menuItems = signal<{ id: number; name: string }[]>([]);
  rawItems = signal<{ id: number; name: string; unit: string }[]>([]);
  selectedMenuItemId = signal<number | null>(null);
  recipeItems = signal<RecipeItem[]>([]);
  loading = signal(false);

  constructor(private menuRecipeService: MenuRecipeService) {
    this.loadMenuItems();
    this.loadRawItems();
  }

  // ⭐ Load Menu Items
  loadMenuItems() {
    this.menuRecipeService.getAllMenuItems()
      .subscribe(items => this.menuItems.set(items));
  }

  // ⭐ Load Raw Items
  loadRawItems() {
    this.menuRecipeService.getAllRawItems()
      .subscribe(items => this.rawItems.set(items));
  }

  // ⭐ Load Recipe for selected menu item
  loadRecipe() {
    if (!this.selectedMenuItemId()) return;

    this.loading.set(true);
    this.menuRecipeService.getRecipeByMenuItemId(this.selectedMenuItemId()!)
      .subscribe({
        next: res => {
          this.recipeItems.set(res.map(r => ({ ...r })));
          this.loading.set(false);
        },
        error: err => {
          alert('Error loading recipe: ' + err.message);
          this.loading.set(false);
        }
      });
  }

  // ⭐ Add new recipe item
  addRecipeItem() {
    const newItem: RecipeItem = { rawItemId: 0, quantityRequired: 0, unit: '' };
    this.recipeItems.update(items => [...items, newItem]);
  }

  // ⭐ Remove recipe item by index
  removeRecipeItem(index: number) {
    this.recipeItems.update(items => items.filter((_, i) => i !== index));
  }

  // ⭐ Save recipe to backend
  saveRecipe() {
  if (!this.selectedMenuItemId()) return alert('Select a menu item');

  const dto: AssignRecipeDto = {
    menuItemId: this.selectedMenuItemId()!,  // <-- non-null assertion
    recipeItems: this.recipeItems().map(item => ({
      rawItemId: item.rawItemId,
      quantityRequired: item.quantityRequired
    }))
  };

  this.menuRecipeService.assignRecipe(dto)
    .subscribe({
      next: () => alert('Recipe saved successfully'),
      error: err => alert('Error saving recipe: ' + (err.message || err))
    });
}

getUnit(rawItemId: number): string {
  const item = this.rawItems().find(x => x.id === rawItemId);
  return item ? item.unit : '';
}

  // ⭐ Delete recipe for selected menu item
  deleteRecipe() {
    if (!this.selectedMenuItemId()) return;
    if (!confirm('Delete recipe for this menu item?')) return;

    this.menuRecipeService.deleteRecipe(this.selectedMenuItemId()!)
      .subscribe({
        next: () => {
          alert('Recipe deleted');
          this.recipeItems.set([]);
        },
        error: err => alert('Error deleting recipe: ' + err.message)
      });
  }

}
