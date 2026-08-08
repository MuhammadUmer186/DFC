import { Injectable, signal } from '@angular/core';

export interface DeliveryLocation {
  lat: number;
  lng: number;
  address: string;
}

const STORAGE_KEY = 'dfc-customer-delivery-location';

@Injectable({ providedIn: 'root' })
export class DeliveryLocationService {
  isOpen = signal(false);
  location = signal<DeliveryLocation | null>(this.loadFromStorage());

  label = () => this.location()?.address ?? '';

  private loadFromStorage(): DeliveryLocation | null {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  }

  open() {
    this.isOpen.set(true);
  }

  close() {
    this.isOpen.set(false);
  }

  setLocation(loc: DeliveryLocation) {
    this.location.set(loc);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(loc));
    this.isOpen.set(false);
  }
}
