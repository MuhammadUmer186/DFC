import { Routes } from '@angular/router';
import { MenuComponent } from './pages/menu/menu.component';
import { CartComponent } from './pages/cart/cart.component';
import { CheckoutComponent } from './pages/checkout/checkout.component';
import { ConfirmationComponent } from './pages/confirmation/confirmation.component';
import { TrackOrderComponent } from './pages/track-order/track-order.component';

export const routes: Routes = [
  { path: '', component: MenuComponent },
  { path: 'cart', component: CartComponent },
  { path: 'checkout', component: CheckoutComponent },
  { path: 'confirmation/:id', component: ConfirmationComponent },
  { path: 'track-order', component: TrackOrderComponent },
  { path: '**', redirectTo: '' }
];
