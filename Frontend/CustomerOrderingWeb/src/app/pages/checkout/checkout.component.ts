import { Component, computed, effect, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CartService } from '../../services/cart.service';
import { OrderService } from '../../services/order.service';
import { OrderTypeService, ServiceType } from '../../services/order-type.service';
import { DeliveryLocationService } from '../../services/delivery-location.service';
import { AreaService, Area } from '../../services/area.service';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.css'
})
export class CheckoutComponent {
  customerName = signal('');
  phoneNumber = signal('');
  address = signal('');
  paymentMethod = signal<'Cash' | 'Online transfer'>('Cash');

  areas = signal<Area[]>([]);
  selectedAreaId = signal<number | null>(null);

  deliveryFee = computed(() => {
    if (this.orderType.serviceType() !== 'Delivery') return 0;
    const areaId = this.selectedAreaId();
    return this.areas().find(a => a.id === areaId)?.deliveryFee ?? 0;
  });
  grandTotal = computed(() => this.cart.totalAmount() + this.deliveryFee());

  submitting = signal(false);
  errorMsg = signal<string | null>(null);

  constructor(
    public cart: CartService,
    public orderType: OrderTypeService,
    public deliveryLocation: DeliveryLocationService,
    private orderService: OrderService,
    private areaService: AreaService,
    private router: Router
  ) {
    effect(() => {
      const loc = this.deliveryLocation.location();
      if (loc && this.requiresAddress) {
        this.address.set(loc.address);
      }
    });

    this.areaService.getAreas().subscribe({
      next: (areas) => this.areas.set(areas),
      error: () => this.errorMsg.set('Could not load delivery areas — please refresh the page.')
    });
  }

  setAreaId(value: string) {
    this.selectedAreaId.set(value ? Number(value) : null);
  }

  get requiresAddress(): boolean {
    return this.orderType.serviceType() === 'Delivery';
  }

  selectServiceType(type: ServiceType) {
    this.orderType.set(type);
  }

  openLocationPicker() {
    this.deliveryLocation.open();
  }

  get canSubmit(): boolean {
    return this.customerName().trim().length > 0
      && this.phoneNumber().trim().length >= 7
      && (!this.requiresAddress || this.address().trim().length > 0)
      && (!this.requiresAddress || this.selectedAreaId() != null)
      && this.cart.lines().length > 0
      && !this.submitting();
  }

  placeOrder() {
    if (!this.canSubmit) return;

    this.submitting.set(true);
    this.errorMsg.set(null);

    const items = this.cart.lines().filter(l => l.kind === 'item').map(l => ({ menuItemId: l.id, quantity: l.quantity }));
    const deals = this.cart.lines().filter(l => l.kind === 'deal').map(l => ({ dealId: l.id, quantity: l.quantity }));

    const loc = this.requiresAddress ? this.deliveryLocation.location() : null;

    this.orderService.placeOrder({
      customerName: this.customerName().trim(),
      phoneNumber: this.phoneNumber().trim(),
      address: this.requiresAddress ? this.address().trim() : '',
      latitude: loc?.lat,
      longitude: loc?.lng,
      paymentMethod: this.paymentMethod(),
      serviceType: this.orderType.serviceType(),
      areaId: this.requiresAddress ? this.selectedAreaId() ?? undefined : undefined,
      items,
      deals
    }).subscribe({
      next: (order) => {
        this.submitting.set(false);
        this.cart.clear();
        this.router.navigate(['/confirmation', order.id], { state: { order } });
      },
      error: (err) => {
        this.submitting.set(false);
        this.errorMsg.set(err?.error || 'Something went wrong placing your order. Please try again.');
      }
    });
  }
}
