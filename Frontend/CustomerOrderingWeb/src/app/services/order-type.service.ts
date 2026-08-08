import { Injectable, signal } from '@angular/core';

export type ServiceType = 'DineIn' | 'Takeaway' | 'Delivery';

const STORAGE_KEY = 'dfc-customer-service-type';

@Injectable({ providedIn: 'root' })
export class OrderTypeService {
  serviceType = signal<ServiceType>(this.loadFromStorage());

  private loadFromStorage(): ServiceType {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (raw === 'DineIn' || raw === 'Takeaway' || raw === 'Delivery') return raw;
    } catch { /* ignore */ }
    return 'Delivery';
  }

  set(type: ServiceType) {
    this.serviceType.set(type);
    localStorage.setItem(STORAGE_KEY, type);
  }
}
