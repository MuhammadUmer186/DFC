import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { PlaceOrderResponse } from '../../services/order.service';
import { ServiceType } from '../../services/order-type.service';
import { RmsCurrencyPipe } from '../../pipes/currency-symbol.pipe';

@Component({
  selector: 'app-confirmation',
  standalone: true,
  imports: [CommonModule, RouterLink, RmsCurrencyPipe],
  templateUrl: './confirmation.component.html',
  styleUrl: './confirmation.component.css'
})
export class ConfirmationComponent {
  orderId: string;
  order: PlaceOrderResponse | null = null;

  constructor(route: ActivatedRoute, router: Router) {
    this.orderId = route.snapshot.paramMap.get('id') ?? '';
    const nav = router.getCurrentNavigation();
    this.order = (nav?.extras?.state?.['order'] as PlaceOrderResponse) ?? (history.state?.order ?? null);
  }

  private get serviceType(): ServiceType {
    return (this.order?.serviceType as ServiceType) ?? 'Delivery';
  }

  get deliveryFee(): number {
    return this.order?.deliveryFeeCharged ?? 0;
  }

  get areaName(): string | undefined {
    return this.order?.areaName;
  }

  get serviceTypeLabel(): string {
    switch (this.serviceType) {
      case 'DineIn': return '🍽️ Dine-in';
      case 'Takeaway': return '🥡 Takeaway';
      default: return '🛵 Delivery';
    }
  }

  get itemsSubtotal(): number {
    if (!this.order) return 0;
    const itemsTotal = this.order.items.reduce((sum, i) => sum + i.unitPrice * i.quantity, 0);
    const dealsTotal = this.order.deals.reduce((sum, d) => sum + d.dealPrice * d.quantity, 0);
    return itemsTotal + dealsTotal;
  }
}
