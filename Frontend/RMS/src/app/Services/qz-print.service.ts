import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';

/**
 * Browser-side printing via QZ Tray (https://qz.io).
 *
 * The .NET backend can only reach the Windows spooler when it runs ON the POS PC.
 * When the API is hosted/online, printing has to happen from the browser instead:
 * QZ Tray is a small app installed on the POS PC that exposes the local printers
 * over a localhost websocket. This service connects to it and sends raw ESC/POS,
 * mirroring Backend/Printing/EscPosReceiptBuilder.cs.
 *
 * Dev/test mode: runs UNSIGNED — QZ Tray shows a one-time "Allow" prompt per
 * site. For production you'd generate a certificate + sign the payloads.
 */

export type PrinterSlot = 'usb1' | 'usb2' | 'ethernet' | 'bluetooth';

export interface QzReceiptItem {
  name: string;
  quantity: number;
  price: number;
}

export interface QzReceiptDto {
  copyType: 'customer' | 'kitchen';
  orderNo: number | string;
  discount: number;
  finalTotal: number;
  items: QzReceiptItem[];
  orderTypeLabel?: string;
  customerName?: string;
  customerPhone?: string;
  customerAddress?: string;
  printedAt?: string;
}

declare const window: any;

const SEP = '==============================================';
const ESC = '\x1B';
const GS = '\x1D';

@Injectable({ providedIn: 'root' })
export class QzPrintService {
  private scriptLoaded?: Promise<void>;
  private securityConfigured = false;

  /** Resolve a logical slot to the configured Windows printer name. */
  printerNameFor(slot: PrinterSlot): string {
    const map = (environment as any).printerNames ?? {};
    return map[slot] ?? '';
  }

  private loadScript(): Promise<void> {
    if (this.scriptLoaded) return this.scriptLoaded;
    this.scriptLoaded = new Promise<void>((resolve, reject) => {
      if (window.qz) return resolve();
      const s = document.createElement('script');
      s.src = 'assets/qz/qz-tray.js';
      s.onload = () => resolve();
      s.onerror = () => reject(new Error('Could not load qz-tray.js'));
      document.head.appendChild(s);
    });
    return this.scriptLoaded;
  }

  private async ensureConnected(): Promise<any> {
    await this.loadScript();
    const qz = window.qz;
    if (!qz) throw new Error('QZ Tray library not available');

    // Unsigned mode — no certificate, empty signature. QZ Tray prompts once.
    if (!this.securityConfigured) {
      qz.security.setCertificatePromise((_resolve: any, reject: any) => reject());
      qz.security.setSignaturePromise((_toSign: string) => (resolve: any) => resolve());
      this.securityConfigured = true;
    }

    if (qz.websocket.isActive()) return qz;
    await qz.websocket.connect({ retries: 2, delay: 1 });
    return qz;
  }

  /** Port of EscPosReceiptBuilder.Build — keep the two in sync. */
  buildEscPos(dto: QzReceiptDto): string {
    const lines: string[] = [];
    const money = (n: number) => n.toFixed(2);

    lines.push(`${ESC}\x40`); // reset

    // header (double size, bold, centered)
    lines.push(`${ESC}\x61\x01${ESC}\x45\x01${GS}\x21\x11DATA FINGER CHIPS${GS}\x21\x00${ESC}\x45\x00`);
    lines.push('Dream Mall Shop# 01 Lalarukh WahCantt');
    lines.push('Cell # 0301-5637195');

    if (dto.orderTypeLabel) {
      lines.push(`${ESC}\x61\x01${ESC}\x45\x01${GS}\x21\x11** ${dto.orderTypeLabel} **${GS}\x21\x00${ESC}\x45\x00`);
    }

    lines.push(dto.copyType === 'customer' ? 'CUSTOMER COPY' : 'KITCHEN COPY');

    lines.push(`${ESC}\x61\x00`); // left
    lines.push(`${ESC}\x61\x00${ESC}\x45\x01${GS}\x21\x11ORDER NO: ${dto.orderNo}${GS}\x21\x00${ESC}\x45\x00`);
    lines.push(`Date: ${dto.printedAt ?? new Date().toLocaleString()}`);

    if (dto.customerName) lines.push(`Customer: ${dto.customerName}`);
    if (dto.customerPhone) lines.push(`WhatsApp: ${dto.customerPhone}`);
    if (dto.customerAddress) lines.push(`Address: ${dto.customerAddress}`);

    lines.push(SEP);

    for (const item of dto.items) {
      lines.push(item.name ?? '');
      const rate = money(item.price).padEnd(8);
      const qty = String(item.quantity).padEnd(8);
      const val = money(item.price * item.quantity).padStart(8);
      lines.push(`Rate: ${rate} Qty: ${qty} Val: Rs ${val}`);
    }

    lines.push(SEP);

    const itemsTotal = dto.items.reduce((s, i) => s + i.price * i.quantity, 0);
    if (dto.discount > 0) {
      lines.push(`Subtotal: Rs ${money(itemsTotal)}`);
      lines.push(`Discount: -Rs ${money(dto.discount)}`);
      lines.push(SEP);
    }

    lines.push(`${ESC}\x45\x01${GS}\x21\x11TOTAL: Rs ${money(dto.finalTotal)}${GS}\x21\x00${ESC}\x45\x00`);

    if (dto.copyType === 'customer') {
      lines.push(`${ESC}\x61\x01`);
      lines.push('Thank you for your order!');
      lines.push('Orbionix Technologies');
      lines.push('Cell: 0328-5107458, 0303-5184773');
      lines.push(`${ESC}\x61\x00`);
    }

    lines.push(`\n\n\n${GS}\x56\x00`); // feed + full cut

    return lines.join('\n');
  }

  /** Connect to QZ Tray and print one receipt to the given slot. */
  async printReceipt(dto: QzReceiptDto, slot: PrinterSlot): Promise<void> {
    const printerName = this.printerNameFor(slot);
    if (!printerName) {
      throw new Error(`No printer name configured for "${slot}" (environment.printerNames)`);
    }
    const qz = await this.ensureConnected();
    const config = qz.configs.create(printerName, { encoding: 'UTF-8' });
    const data = [{ type: 'raw', format: 'command', flavor: 'plain', data: this.buildEscPos(dto) }];
    await qz.print(config, data);
  }
}
