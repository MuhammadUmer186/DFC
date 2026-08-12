import { Component, OnInit } from '@angular/core';

import { FormsModule } from '@angular/forms';

import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import { RmsCurrencyPipe } from '../../Shared/pipes/currency-symbol.pipe';

export interface MenuItem {
  id: number;
  name: string;
  price: number;
  image: string;
  description: string;
}

export interface OrderItem {
  menuItem: MenuItem;
  quantity: number;
}

@Component({
  selector: 'app-menu',
  standalone: true,
  imports: [FormsModule, RmsCurrencyPipe],
  templateUrl: './menu.component.html',
  styleUrl: './menu.component.css'
})
export class MenuComponent implements OnInit {
  menuItems: MenuItem[] = [];
  selectedItems: OrderItem[] = [];
  totalBill: number = 0;

  ngOnInit(): void {
    this.loadMenuItems();
  }

  loadMenuItems(): void {
    this.menuItems = [
      { id: 1, name: 'French Fries', price: 150, image: 'assets/images/fries.jpg', description: 'Crispy golden fries' },
      { id: 2, name: 'Zinger Burger', price: 350, image: 'assets/burger.jpg', description: 'Spicy chicken burger' },
      { id: 3, name: 'Shawarma', price: 300, image: 'assets/images/shawarma.jpg', description: 'Grilled meat wrap' },
      { id: 4, name: 'Pizza', price: 500, image: 'assets/images/pizza.jpg', description: 'Cheesy delicious pizza' },
      { id: 5, name: 'Loaded Fires', price: 400, image: 'assets/Loaded Fries.jpg', description: 'Crunchy and Cheesy' },
      { id: 6, name: 'Mint Margrita', price: 80, image: 'assets/Mint Margrita.jpg', description: 'Coldness at its Peak' }
    ];
  }
  // ...existing code...
  // replaced generatePdf()
  // ...existing code...
  // improved 80mm slip generator (replace existing generatePdf)
  // ...existing code...
// ...existing code...

  // helper: load image from assets and convert to data URL
  // ...existing code...

  // helper: load image from assets and convert to data URL
  private async imageToDataUrl(url: string): Promise<string> {
    const res = await fetch(url);
    if (!res.ok) throw new Error('Image load failed');
    const blob = await res.blob();
    return await new Promise<string>((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => resolve(reader.result as string);
      reader.onerror = () => reject(reader.error);
      reader.readAsDataURL(blob);
    });
  }

  // generic PDF generator with copyType parameter
  // ...existing code...

  // generic PDF generator with copyType parameter - ROLL STYLE (continuous, no page breaks)
  private async generateReceiptPdf(copyType: 'customer' | 'kitchen'): Promise<void> {
    if (this.selectedItems.length === 0) {
      alert('No items selected to generate receipt.');
      return;
    }

    const slipWidthMm = 80;
    const marginLeft = 4;
    const marginRight = 4;
    const usableWidth = slipWidthMm - marginLeft - marginRight;
    const logoUrl = '/assets/images/logo.png';
    let logoDataUrl: string | null = null;
    let logoHeightMm = 0;
    const logoWidthMm = 30;

    // try load logo (optional)
    try {
      logoDataUrl = await this.imageToDataUrl(logoUrl);
      logoHeightMm = 12;
    } catch {
      logoDataUrl = null;
      logoHeightMm = 0;
    }

    // calculate exact height based on content (roll-style continuous)
    let estimatedHeight = 10; // start padding
    if (logoDataUrl) estimatedHeight += logoHeightMm + 2;
    estimatedHeight += 5; // copy type header
    estimatedHeight += 5; // restaurant name space
    estimatedHeight += 12; // date/receipt info
    estimatedHeight += 8; // table header
    estimatedHeight += this.selectedItems.length * 6; // rows (compact)
    estimatedHeight += 8; // total section
    if (copyType === 'customer') estimatedHeight += 12; // footer
    estimatedHeight += 10; // end padding

    // create doc with ROLL format (80mm width x calculated height)
    const doc = new jsPDF({ 
      unit: 'mm', 
      format: [slipWidthMm, estimatedHeight] 
    });

    // draw logo if available
    let cursorY = 5;
    if (logoDataUrl) {
      const x = (slipWidthMm - logoWidthMm) / 2;
      try { 
        doc.addImage(logoDataUrl, 'PNG', x, cursorY, logoWidthMm, logoHeightMm); 
        cursorY += logoHeightMm + 2;
      }
      catch { /* ignore if image fails */ }
    }

    // copy type header (CUSTOMER COPY or KITCHEN COPY)
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(10);
    const copyText = copyType === 'customer' ? 'CUSTOMER COPY' : 'KITCHEN COPY';
    doc.text(copyText, slipWidthMm / 2, cursorY, { align: 'center' });
    cursorY += 5;

    // restaurant name
    doc.setFontSize(9);
    doc.text('Data Finger Chips', slipWidthMm / 2, cursorY, { align: 'center' });
    cursorY += 5;

    // date and receipt info
    doc.setFontSize(7);
    doc.setFont('helvetica', 'normal');
    const now = new Date();
    doc.text(`Date: ${now.toLocaleString()}`, marginLeft, cursorY); 
    cursorY += 3;
    doc.text(`Receipt #: ${now.getTime()}`, marginLeft, cursorY); 
    cursorY += 3;
    doc.text('----------------------------------------', marginLeft, cursorY); 
    cursorY += 4;

    // build table data (Item, Qty, Unit, Total)
    const head = [['Item', 'Qty', 'Unit', 'Total']];
    const body = this.selectedItems.map(it => {
      const qty = Number(it.quantity) || 1;
      const unit = Number(it.menuItem.price) || 0;
      const total = unit * qty;
      return [it.menuItem.name, qty.toString(), unit.toString(), total.toString()];
    });

    // use autoTable with explicit mm widths (compact for roll)
    (autoTable as any)(doc, {
      startY: cursorY,
      head,
      body,
      theme: 'plain',
      styles: { fontSize: 6.5, cellPadding: 0.8 },
      headStyles: { fillColor: [240, 240, 240], textColor: [0, 0, 0], fontStyle: 'bold' },
      columnStyles: {
        0: { cellWidth: 36, halign: 'left' },
        1: { cellWidth: 8, halign: 'center' },
        2: { cellWidth: 14, halign: 'right' },
        3: { cellWidth: 14, halign: 'right' }
      },
      margin: { left: marginLeft, right: marginRight },
      bodyStyles: { valign: 'middle' },
      didDrawPage: () => {} // prevent page breaks
    });

    const finalY = (doc as any).lastAutoTable ? (doc as any).lastAutoTable.finalY + 3 : cursorY + (this.selectedItems.length * 6);

    // divider
    doc.setFontSize(7);
    doc.text('----------------------------------------', marginLeft, finalY);

    // total
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(9);
    doc.text(`TOTAL: Rs. ${this.totalBill}`, slipWidthMm - marginRight, finalY + 4, { align: 'right' });

    // footer (centered, wrapped) - only for customer copy
    if (copyType === 'customer') {
      const footerStartY = finalY + 8;
      doc.setFont('helvetica', 'normal');
      doc.setFontSize(6.5);
      const footerLines = ['Thank you for your order!', 'Orbon Technologies', 'Contact# 0328-5107458, 0303-5184773'];
      let fy = footerStartY;
      footerLines.forEach(line => {
        const wrapped = (doc as any).splitTextToSize(line, usableWidth);
        wrapped.forEach((ln: string) => { 
          doc.text(ln, slipWidthMm / 2, fy, { align: 'center' }); 
          fy += 3; 
        });
      });
    }

    const fileName = `${copyType}_receipt_${now.getTime()}.pdf`;
    doc.save(fileName);
  }

  // ...rest of existing code...
  // public methods called from template
  async generateCustomerCopy(): Promise<void> {
    await this.generateReceiptPdf('customer');
  }

  async generateKitchenCopy(): Promise<void> {
    await this.generateReceiptPdf('kitchen');
  }

  async generatePdf(): Promise<void> {
    await this.generateCustomerCopy();
  }

// ...existing code...

// ...existing code...
// ...existing code...
// ...existing code...
// ...existing code...
  addToOrder(item: MenuItem): void {
    const existingItem = this.selectedItems.find(o => o.menuItem.id === item.id);
    if (existingItem) {
      existingItem.quantity++;
    } else {
      this.selectedItems.push({ menuItem: item, quantity: 1 });
    }
    this.calculateTotal();
  }

  removeFromOrder(id: number): void {
    this.selectedItems = this.selectedItems.filter(item => item.menuItem.id !== id);
    this.calculateTotal();
  }

  updateQuantity(id: number, quantity: number): void {
    const item = this.selectedItems.find(o => o.menuItem.id === id);
    if (item && quantity > 0) {
      item.quantity = quantity;
      this.calculateTotal();
    }
  }

  calculateTotal(): void {
    this.totalBill = this.selectedItems.reduce((sum, item) => sum + (item.menuItem.price * item.quantity), 0);
  }

  submitOrder(): void {
    if (this.selectedItems.length === 0) {
      alert('Please select items before submitting!');
      return;
    }
    console.log('Order submitted:', this.selectedItems);
    alert('Order submitted successfully!');
    this.selectedItems = [];
    this.totalBill = 0;
  }
}