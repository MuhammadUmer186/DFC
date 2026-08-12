import { Component, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CartService } from '../../services/cart.service';
import { CartLine } from '../../models/cart.model';
import { OrderTypeService } from '../../services/order-type.service';
import { RmsCurrencyPipe } from '../../pipes/currency-symbol.pipe';

@Component({
  selector: 'app-cart-panel',
  standalone: true,
  imports: [CommonModule, RouterLink, RmsCurrencyPipe],
  templateUrl: './cart-panel.component.html',
  styleUrl: './cart-panel.component.css'
})
export class CartPanelComponent {
  // Delivery fee depends on the area picked at checkout, so this preview
  // only ever shows the item subtotal — the real total is shown at checkout.
  grandTotal = computed(() => this.cart.totalAmount());

  constructor(public cart: CartService, public orderType: OrderTypeService) {}

  increase(line: CartLine) {
    this.cart.setQuantity(line.kind, line.id, line.quantity + 1);
  }

  decrease(line: CartLine) {
    this.cart.setQuantity(line.kind, line.id, line.quantity - 1);
  }

  remove(line: CartLine) {
    this.cart.remove(line.kind, line.id);
  }

  clearCart() {
    if (!confirm('Clear your whole cart?')) return;
    this.cart.clear();
  }
}
