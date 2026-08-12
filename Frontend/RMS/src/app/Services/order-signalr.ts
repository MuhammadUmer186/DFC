import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../environments/environment';
 // create interface file if needed
export interface OrderQueueDto {
  id: number;
  orderNumber?: string | null;
  totalAmount: number;
  createdAt: string;
  paid: boolean;
  discount?: number;
  takenByEmployeeName: string;
  customerName?: string | null;
  phoneNumber?: string | null;
  address?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  orderSource?: string | null;
  serviceType?: string | null;
  paymentMethod?: string | null;
  riderId?: number | null;
  riderName?: string | null;
  riderCost?: number | null;
  areaId?: number | null;
  areaName?: string | null;
  deliveryFeeCharged?: number | null;
  deliveryStatus?: 'Approved' | 'Preparing' | 'Enroute' | 'Delivered' | 'Rejected' | null;
  items: { menuItemName: string; unitPrice: number; quantity: number }[];
  deals: { dealName: string; dealPrice: number; quantity: number; items: { name: string; price: number; quantity: number }[] }[];
  expanded?: boolean;
  totalQuantity?: number;
}
@Injectable({ providedIn: 'root' })
export class OrderSignalRService {
  private baseUrll = environment.apihub;
  private baseUrl = this.baseUrll + '/hubs/orders';
  private hubConnection?: signalR.HubConnection;
  private listenersRegistered = false;

  // Multiple components (Sidebar, Queue, Online Orders) can all be alive at once and
  // all call startConnection()/stopConnection() independently — this is a single shared
  // connection, so we reference-count it rather than let any one caller tear it down
  // out from under the others.
  private consumerCount = 0;

  queuedOrders = signal<OrderQueueDto[]>([]);
  newOnlineOrders = signal<OrderQueueDto[]>([]);
  onlineOrderResolved = signal<number | null>(null); // orderId of the most recently approved/rejected online order
  riderAssigned = signal<OrderQueueDto | null>(null); // most recently rider-assigned order
  deliveryStatusUpdated = signal<OrderQueueDto | null>(null); // most recently status-updated order

  // Bumped on every order-lifecycle event, regardless of type — anything that wants to
  // auto-refresh whenever "any order activity happened anywhere" (e.g. the sidebar's
  // Today's Summary widget) can just watch this one signal instead of the specific ones.
  lastActivityAt = signal<number>(0);

  startConnection() {
    this.consumerCount++;

    if (this.hubConnection && this.hubConnection.state !== signalR.HubConnectionState.Disconnected) {
      return; // already connected/connecting — nothing to do
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.baseUrl)
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start()
      .then(() => {
        console.log('✅ SignalR connected');
        this.hubConnection!.invoke('JoinQueue');
        this.registerListeners();
      })
      .catch(err => console.error('SignalR error:', err));
  }

  stopConnection() {
    this.consumerCount = Math.max(0, this.consumerCount - 1);
    if (this.consumerCount === 0) {
      this.hubConnection?.stop();
      this.listenersRegistered = false;
    }
  }

  private registerListeners() {
    if (this.listenersRegistered || !this.hubConnection) return;
    this.listenersRegistered = true;

    // Fired for every order regardless of paid status (e.g. a normal walk-in POS sale
    // that's paid immediately never touches OrderQueued at all) — this is the one to
    // watch for "something happened" style auto-refreshes.
    this.hubConnection.on('OrderCreated', () => {
      this.lastActivityAt.set(Date.now());
    });

    this.hubConnection.on('OrderQueued', (order: OrderQueueDto) => {
      this.queuedOrders.update(list => [{ ...order, expanded: false }, ...list]);
      this.lastActivityAt.set(Date.now());
    });

    this.hubConnection.on('OrderPaid', (orderId: number) => {
      this.queuedOrders.update(list => list.filter(o => o.id !== orderId));
      this.lastActivityAt.set(Date.now());
    });

    this.hubConnection.on('OrderCancelled', (orderId: number) => {
      this.queuedOrders.update(list => list.filter(o => o.id !== orderId));
      this.lastActivityAt.set(Date.now());
    });

    this.hubConnection.on('NewOnlineOrder', (order: OrderQueueDto) => {
      this.newOnlineOrders.update(list => [{ ...order, expanded: false }, ...list]);
      this.lastActivityAt.set(Date.now());
    });

    this.hubConnection.on('OnlineOrderApproved', (orderId: number) => {
      this.newOnlineOrders.update(list => list.filter(o => o.id !== orderId));
      this.onlineOrderResolved.set(orderId);
      this.lastActivityAt.set(Date.now());
    });

    this.hubConnection.on('OnlineOrderRejected', (orderId: number) => {
      this.newOnlineOrders.update(list => list.filter(o => o.id !== orderId));
      this.onlineOrderResolved.set(orderId);
      this.lastActivityAt.set(Date.now());
    });

    this.hubConnection.on('OrderRiderAssigned', (order: OrderQueueDto) => {
      this.newOnlineOrders.update(list => list.map(o => o.id === order.id ? { ...o, riderId: order.riderId, riderName: order.riderName, riderCost: order.riderCost } : o));
      this.riderAssigned.set(order);
      this.lastActivityAt.set(Date.now());
    });

    this.hubConnection.on('OrderDeliveryStatusUpdated', (order: OrderQueueDto) => {
      this.deliveryStatusUpdated.set(order);
      this.lastActivityAt.set(Date.now());
    });
  }
}
