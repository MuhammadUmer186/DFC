import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { CartService } from './services/cart.service';
import { LocationPickerModalComponent } from './components/location-picker-modal/location-picker-modal.component';
import { WhatsappFloatComponent } from './components/whatsapp-float/whatsapp-float.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, LocationPickerModalComponent, WhatsappFloatComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  constructor(public cart: CartService) {}
}
