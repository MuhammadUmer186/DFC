import { Injectable, effect, signal } from '@angular/core';
import { PrinterSlot } from './qz-print.service';

/**
 * Which printer slot the customer copy vs the kitchen copy goes to.
 * Persisted to localStorage so the choice survives a reload, and shared by
 * every screen that prints (Menu Order, Queue, ...). Swapping the two values
 * swaps which physical printer each copy comes out of.
 */

const KEY = 'rms.printerRouting';
const DEFAULTS: { customer: PrinterSlot; kitchen: PrinterSlot } = {
  customer: 'usb1',
  kitchen: 'usb2',
};

@Injectable({ providedIn: 'root' })
export class PrinterSettingsService {
  readonly customerSlot = signal<PrinterSlot>(DEFAULTS.customer);
  readonly kitchenSlot = signal<PrinterSlot>(DEFAULTS.kitchen);

  constructor() {
    try {
      const saved = JSON.parse(localStorage.getItem(KEY) || '{}');
      if (saved.customer) this.customerSlot.set(saved.customer);
      if (saved.kitchen) this.kitchenSlot.set(saved.kitchen);
    } catch {
      /* ignore corrupt / unavailable storage */
    }

    // Persist on any change.
    effect(() => {
      const value = { customer: this.customerSlot(), kitchen: this.kitchenSlot() };
      try {
        localStorage.setItem(KEY, JSON.stringify(value));
      } catch {
        /* storage full / disabled — in-memory still works for this session */
      }
    });
  }

  setCustomer(slot: PrinterSlot) {
    this.customerSlot.set(slot);
  }

  setKitchen(slot: PrinterSlot) {
    this.kitchenSlot.set(slot);
  }
}
