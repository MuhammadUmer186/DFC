import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../environments/environment';
 // create interface file if needed
export interface OrderQueueDto {
  id: number;
  totalAmount: number;
  createdAt: string;
  paid: boolean;
  discount?: number;
  takenByEmployeeName: string;
  items: { menuItemName: string; unitPrice: number; quantity: number }[];
  deals: { dealName: string; dealPrice: number; quantity: number; items: { name: string; price: number; quantity: number }[] }[];
  expanded?: boolean;
  totalQuantity?: number;
}
@Injectable({ providedIn: 'root' })
export class OrderSignalRService {
  private baseUrll = environment.apihub;
  private baseUrl = this.baseUrll + '/hubs/orders';
  private hubConnection!: signalR.HubConnection;
  queuedOrders = signal<OrderQueueDto[]>([]);

  startConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.baseUrl)
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start()
      .then(() => {
        console.log('✅ SignalR connected');
        this.hubConnection.invoke('JoinQueue');
        this.registerListeners();
      })
      .catch(err => console.error('SignalR error:', err));
  }

  stopConnection() {
    this.hubConnection?.stop();
  }

  private registerListeners() {
    this.hubConnection.on('OrderQueued', (order: OrderQueueDto) => {
      this.queuedOrders.update(list => [{ ...order, expanded: false }, ...list]);
    });

    this.hubConnection.on('OrderPaid', (orderId: number) => {
      this.queuedOrders.update(list => list.filter(o => o.id !== orderId));
    });

    this.hubConnection.on('OrderCancelled', (orderId: number) => {
      this.queuedOrders.update(list => list.filter(o => o.id !== orderId));
    });
  }
}
