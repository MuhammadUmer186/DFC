import { Component } from '@angular/core';
import { CartPanelComponent } from '../../components/cart-panel/cart-panel.component';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CartPanelComponent],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.css'
})
export class CartComponent {}
