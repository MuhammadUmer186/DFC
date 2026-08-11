import { OrderQueueDto } from './order-signalr';

export type DeliveryStatusValue = 'Approved' | 'Preparing' | 'Enroute' | 'Delivered' | 'Rejected';

// Fulfillment moves one step at a time — Approved → Preparing → (Enroute →) Delivered.
// Delivery orders pass through Enroute (rider on the way); pickup/dine-in orders skip it.
// Mirrors OrderService.GetNextDeliveryStatus on the backend, which is the source of truth.
export function nextDeliveryStatus(order: OrderQueueDto): DeliveryStatusValue | null {
  const isDelivery = (order.serviceType ?? 'Delivery') === 'Delivery';
  switch (order.deliveryStatus) {
    case 'Approved': return 'Preparing';
    case 'Preparing': return isDelivery ? 'Enroute' : 'Delivered';
    case 'Enroute': return isDelivery ? 'Delivered' : null;
    default: return null;
  }
}
