import { Component, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { KitchenOrderService } from '../../Services/kitchenorder.service';
import { PrintService } from '../../Services/printservice';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../Services/toast.service';
import { AuthService } from '../../Services/auth.service';
import { environment } from '../../../environments/environment';
//declare var qz: any;
@Component({
  standalone: true,
  selector: 'app-menu-order',
  imports: [CommonModule, FormsModule],
  templateUrl: './menu-order.component.html',
  styleUrls: ['./menu-order.component.css']
})
export class MenuOrderComponent {

  // =========================
  // STATE (Signals)
  // =========================
  DEALS_CATEGORY_ID = -1;
  displayItems = signal<any[]>([]);
  searchText = signal('');
  searchResults = signal<any[]>([]);
  EXTRA_MENU_ITEM_ID = 204; // 👈 must match DB Extra Charge menu item
  extraPrices = [10,20,30,40,50,60,70,80,90,100];
  selectedExtraPrice = signal<number | null>(null);
  categories = signal<any[]>([]);
  menuItems = signal<any[]>([]);
  dealsMaster = signal<any[]>([]);

  cart = signal<any[]>([]);
  appliedDeals = signal<any[]>([]);

  selectedCategoryId = signal(0);

  discountPercent = signal(0);
  touchMode = signal(false); // enable by default for POS


  // =========================
  // DERIVED STATE (Computed)
  // =========================

  itemsTotal = computed(() =>
    this.cart().reduce((a, b) => a + (b.total || 0), 0)
  );

  dealsTotal = computed(() =>
    this.appliedDeals().reduce((a, d) => a + d.dealPrice * d.quantity, 0)
  );

  discountAmount = computed(() =>
    this.discountPercent() > 0
      ? (this.itemsTotal() * this.discountPercent()) / 100
      : 0
  );

  totalAmount = computed(() =>
    this.itemsTotal() + this.dealsTotal()
  );

  finalTotal = computed(() =>
    (this.itemsTotal() - this.discountAmount()) + this.dealsTotal()
  );

  totalQuantity = computed(() =>
    this.cart().reduce((a, b) => a + b.quantity, 0) +
    this.appliedDeals().reduce((a, d) => a + d.quantity, 0)
  );

  constructor(
    private orderService: KitchenOrderService,
    private toast: ToastService,
    private printService: PrintService,
    private auth:AuthService
  ) {
    this.loadCategories();
    this.loadDeals();
  //   if (!qz.websocket.isActive()) {
  //   qz.websocket.connect()
  //     .then(() => console.log("QZ Tray connected"))
  //     .catch((err:any) => console.error(err));
  // }
  
   
  }

  // =========================
  // DATA LOAD
  // =========================
getCategoryIcon(name: string): string {
  const n = name.toLowerCase();

  if (n.includes('fries') || n.includes('fry')) return '🍟';
  if (n.includes('burger')) return '🍔';
  if (n.includes('shawarma') || n.includes('wrap') || n.includes('roll')) return '🌯';
  if (n.includes('pizza')) return '🍕';
  if (n.includes('chicken')) return '🍗';
  if (n.includes('rice') || n.includes('biryani')) return '🍛';
  if (n.includes('beverage') || n.includes('drink') || n.includes('juice') || n.includes('margarita')) return '🥤';
  if (n.includes('dessert') || n.includes('sweet') || n.includes('ice')) return '🍨';
  if (n.includes('side') || n.includes('extra') || n.includes('dip') || n.includes('sauce')) return '🍢';

  return '🍽️';
}

fullImageUrl(imageUrl?: string | null): string | null {
  if (!imageUrl) return null;
  return `${environment.apihub}${imageUrl}`;
}

  loadCategories() {
    this.orderService.getCategories()
      .subscribe(res => this.categories.set(res));
  }

  loadDeals() {
    this.orderService.getDeals()
      .subscribe(res => this.dealsMaster.set(res));
  }

  selectCategory(id: number) {
  this.selectedCategoryId.set(id);

  // DEALS CATEGORY
  if (id === this.DEALS_CATEGORY_ID) {
    this.displayItems.set(
      this.dealsMaster().map(d => ({
        ...d,
        _type: 'deal'
      }))
    );
    return;
  }

  // NORMAL CATEGORY
  this.orderService.getMenuItems(id)
    .subscribe(res => {
      this.displayItems.set(
        res.map((m: any) => ({
          ...m,
          _type: 'item'
        }))
      );
    });
}


searchMenuItems() {

  const term = this.searchText().trim();

  // If search box empty
  if (!term) {
    this.searchResults.set([]);
    return;
  }

  this.orderService.searchMenuItems(term)
    .subscribe(res => {

      this.searchResults.set(
        res.map((m: any) => ({
          ...m,
          _type: 'item'
        }))
      );

    });
}
onSearchChange(value: string) {
  this.searchText.set(value);

  // call backend search
  this.searchMenuItems();
}

  // =========================
  // CART OPERATIONS
  // =========================

  addToCart(item: any) {
    this.cart.update(cart => {
      const existing = cart.find(x => x.menuItemId === item.id);

      if (existing) {
        existing.quantity++;
        existing.total = existing.unitPrice * existing.quantity;
        return [...cart];
      }

      return [
        ...cart,
        {
          menuItemId: item.id,
          name: item.name,
          unitPrice: item.price,
          quantity: 1,
          total: item.price,
          type: 'item',
          imageUrl: item.imageUrl
        }
      ];
    });
  }

  increase(row: any) {
    row.quantity++;
    if (row.type === 'item') {
      row.total = row.unitPrice * row.quantity;
      this.cart.set([...this.cart()]);
    } else {
      this.appliedDeals.set([...this.appliedDeals()]);
    }
  }

  decrease(row: any) {
    if (row.quantity > 1) {
      row.quantity--;
      if (row.type === 'item') row.total = row.unitPrice * row.quantity;
    } else {
      if (row.type === 'item') {
        this.cart.set(this.cart().filter(x => x !== row));
        return;
      }
      this.appliedDeals.set(this.appliedDeals().filter(d => d !== row));
      return;
    }

    row.type === 'item'
      ? this.cart.set([...this.cart()])
      : this.appliedDeals.set([...this.appliedDeals()]);
  }

  removeLine(row: any) {
    if (row.type === 'item') {
      this.cart.set(this.cart().filter(x => x !== row));
    } else {
      this.appliedDeals.set(this.appliedDeals().filter(d => d !== row));
    }
  }

  applyDeal(deal: any) {
    this.appliedDeals.update(deals => {
      const existing = deals.find(d => d.dealId === deal.id);

      if (existing) {
        existing.quantity++;
        return [...deals];
      }

      return [
        ...deals,
        {
          dealId: deal.id,
          dealName: deal.dealName,
          dealPrice: deal.finalPrice,
          quantity: 1,
          type: 'deal'
        }
      ];
    });
  }
toggleFullscreen() {
  if (!document.fullscreenElement) {
    document.documentElement.requestFullscreen();
  } else {
    document.exitFullscreen();
  }
}

  applyDiscount() {
    this.toast.info(`Discount applied: ${this.discountPercent()}%`);
  }

  // =========================
  // PLACE ORDER
  // =========================
 addExtraCharge() {
  const price = Number(this.selectedExtraPrice());

  if (!price || price <= 0) {
    this.toast.warn("Please select extra amount");
    return;
  }

  this.cart.update(cart => [
    ...cart,
    {
      menuItemId: this.EXTRA_MENU_ITEM_ID,
      name: 'Extra Charge',
      unitPrice: price,
      quantity: 1,
      total: price,
      type: 'item',
      isExtra: true
    }
  ]);

  this.selectedExtraPrice.set(null);
}
  placeOrder() {

  // 🚫 Validation
  if (this.cart().length === 0 && this.appliedDeals().length === 0) {
    this.toast.warn("No items or deals selected");
    return;
  }

  const itemsTotal = this.itemsTotal();
  const dealsTotal = this.dealsTotal();
  const discountAmount = this.discountAmount();
  const finalTotal = this.finalTotal();

  const orderRequest = {
    items: this.cart().map(x => ({
    menuItemId: x.menuItemId,
    quantity: x.quantity,
    unitPriceOverride: x.isExtra ? x.unitPrice : undefined
  })),
    deals: this.appliedDeals().map(d => ({
      dealId: d.dealId,
      quantity: d.quantity
    }))
  };

  // 🟢 PLACE ORDER
  this.orderService.placeOrder(orderRequest, this.discountPercent())
    .subscribe({
      next: (order: any) => {

        // 🧾 BUILD RECEIPT ITEMS (same structure as QueueComponent)
        const receiptItems = [
          ...this.cart().map(i => ({
            name: i.name,
            quantity: i.quantity,
            price: i.unitPrice
          })),
          ...this.appliedDeals().map(d => ({
            name: d.dealName,
            quantity: d.quantity,
            price: d.dealPrice
          }))
        ];

        // 🖨 CUSTOMER COPY
        this.printService.printReceipt({
          copyType: 'customer',
          orderNo: order.id,
          total: itemsTotal + dealsTotal, // subtotal before discount
          discount: discountAmount,
          finalTotal: finalTotal,
          items: receiptItems
        }, 'usb1').subscribe({
          error: () => this.toast.error('Customer print failed')
        });

        // 🖨 KITCHEN COPY
        this.printService.printReceipt({
          copyType: 'kitchen',
          orderNo: order.id,
          total: itemsTotal + dealsTotal,
          discount: 0,                    // kitchen copy ignores discount
          finalTotal: finalTotal,
          items: receiptItems
        }, 'usb2').subscribe({
          error: () => this.toast.error('Kitchen print failed')
        });

        // 🔄 RESET CART
        this.cart.set([]);
        this.appliedDeals.set([]);
        this.discountPercent.set(0);

        this.toast.success("Order placed & printed successfully!");
      },
      error: (err) => {
        console.error(err);
        this.toast.error("Failed to place order!");
      }
    });
}

// =========================
// SEND ORDER TO QUEUE (UNPAID)
// =========================
sendToQueue() {

  // 🚫 Validation
  if (this.cart().length === 0 && this.appliedDeals().length === 0) {
    this.toast.warn("No items or deals selected");
    return;
  }

  const orderRequest = {
    items: this.cart().map(x => ({
      menuItemId: x.menuItemId,
      quantity: x.quantity,
      unitPriceOverride: x.isExtra ? x.unitPrice : undefined
    })),
    deals: this.appliedDeals().map(d => ({
      dealId: d.dealId,
      quantity: d.quantity
    })),
    paid: false // 👈 send to queue as unpaid
  };

  // 🟢 PLACE ORDER IN QUEUE
  this.orderService.placeOrder(orderRequest, this.discountPercent())
    .subscribe({
      next: (order: any) => {

        this.toast.success(`Order #${order.orderNumber || order.id} sent to queue!`);

        // 🔄 RESET CART
        this.cart.set([]);
        this.appliedDeals.set([]);
        this.discountPercent.set(0);
      },
      error: (err) => {
        console.error(err);
        this.toast.error("Failed to send order to queue!");
      }
    });
}



// // printReceipt(printerName: string, receiptText: string) {
// //   const config = qz.configs.create(printerName, {
// //     encoding: 'UTF-8',
// //     rasterize: false
// //   });

// //   const data = [{
// //     type: 'raw',
// //     format: 'command',
// //     data:
// //       '\x1B\x40' +            // INIT
// //       receiptText +
// //       '\n\n\n\n\n' +          // IMPORTANT: FEED PAPER
// //       '\x1D\x56\x42\x00'      // PARTIAL CUT (Xprinter)
// //   }];

// //   qz.print(config, data)
// //     .then(() => console.log(`Printed & cut on ${printerName}`))
// //     .catch((err: any) => console.error("Print failed", err));
// // }


//   // =========================
//   // RECEIPT (UNCHANGED LOGIC)
//   // =========================

//  generateReceipt(type: 'customer' | 'kitchen') {
//   const cart = this.cart();
//   const deals = this.appliedDeals();
//   const totalAmount = this.totalAmount();
//   const discountPercent = this.discountPercent();
//   const discountAmount = this.discountAmount();
//   const finalTotal = this.finalTotal();

//   if (cart.length === 0 && deals.length === 0) {
//     alert("No items or deals found!");
//     return;
//   }

//   const receiptWidth = 80;
//   const baseHeight = 80 + (cart.length + deals.length) * 8;

//   const doc = new jsPDF({
//     unit: "mm",
//     format: [receiptWidth, baseHeight],
//   });

//   let y = 6;

//   doc.setFont('helvetica', 'bold');
//   doc.setFontSize(14);
//   doc.text("DATA FINGER CHIPS", receiptWidth / 2, y, { align: "center" });
//   y += 6;

//   doc.setFontSize(9);
//   doc.text(
//     type === 'customer' ? "CUSTOMER COPY" : "KITCHEN COPY",
//     receiptWidth / 2,
//     y,
//     { align: "center" }
//   );
//   y += 6;

//   doc.setFontSize(8);
//   doc.text("Date: " + new Date().toLocaleString(), 4, y);
//   y += 4;

//   doc.text("--------------------------------------", 4, y);
//   y += 3;

//   // ✅ Table body including items + deals
//   const body = [
//     ...cart.map(x => [x.name, x.quantity.toString(), x.unitPrice.toString(), x.total.toString()]),
//     ...deals.map(d => [d.dealName, d.quantity.toString(), d.dealPrice.toString(), (d.dealPrice * d.quantity).toString()])
//   ];

//   // ✅ Generate table
//   (autoTable as any)(doc, {
//     startY: y,
//     head: [['Item', 'Qty', 'Unit', 'Total']],
//     body,
//     theme: "plain",
//     styles: { fontSize: 8 },
//     headStyles: { fontStyle: 'bold' }
//   });

//   // ✅ Get final Y after table
//   const lastTable = (doc as any).lastAutoTable;
//   let finalY = lastTable?.finalY ? lastTable.finalY + 5 : y + 10;

//   doc.setFontSize(9);
//   doc.setFont('helvetica', 'bold');

//   // 🔥 SUBTOTAL
//   doc.text(
//     "Subtotal: Rs " + totalAmount,
//     receiptWidth - 4,
//     finalY,
//     { align: "right" }
//   );
//   finalY += 5;

//   // 🔥 DISCOUNT
//   if (discountPercent > 0) {
//     doc.text(
//       `Discount (${discountPercent}%): -Rs ${discountAmount}`,
//       receiptWidth - 4,
//       finalY,
//       { align: "right" }
//     );
//     finalY += 5;

//     doc.text("--------------------------------------", 4, finalY);
//     finalY += 4;

//     doc.text(
//       "FINAL TOTAL: Rs " + finalTotal,
//       receiptWidth - 4,
//       finalY,
//       { align: "right" }
//     );
//     finalY += 6;
//   } else {
//     doc.text(
//       "TOTAL: Rs " + totalAmount,
//       receiptWidth - 4,
//       finalY,
//       { align: "right" }
//     );
//     finalY += 6;
//   }

//   // 🔥 FOOTER for customer
//   if (type === 'customer') {
//     doc.setFontSize(8);
//     doc.setFont('helvetica', 'normal');

//     doc.text("--------------------------------------", 4, finalY);
//     finalY += 4;

//     doc.text("Thank you for your order", receiptWidth / 2, finalY, { align: "center" });
//     finalY += 4;

//     doc.text("Robionix Technologies", receiptWidth / 2, finalY, { align: "center" });
//     finalY += 4;

//     doc.text("Cell: 0328-5107458,0303-5184773", receiptWidth / 2, finalY, { align: "center" });
//   }

//   doc.save(`${type}_copy_${Date.now()}.pdf`);
// }

// generatePosReceipt(order: any, type: 'customer' | 'kitchen'): string {
//   const lines: string[] = [];

//   lines.push("DATA FINGER CHIPS");
//   lines.push(type === 'customer' ? "CUSTOMER COPY" : "KITCHEN COPY");
//   lines.push("Date: " + new Date().toLocaleString());
//   lines.push("--------------------------------");

//   // NORMAL ITEMS
//   order.orderItems?.forEach((i: any) => {
//     lines.push(
//       `${i.menuItem?.name ?? 'Item'} x${i.quantity}  Rs ${i.unitPrice}`
//     );
//   });

//   // DEALS
//   order.orderDeals?.forEach((d: any) => {
//     lines.push(
//       `${d.deal?.dealName ?? 'Deal'} x${d.quantity}  Rs ${d.dealPrice}`
//     );
//   });

//   lines.push("--------------------------------");
//   lines.push(`TOTAL: Rs ${order.totalAmount}`);

//   if (type === 'customer') {
//     lines.push("--------------------------------");
//     lines.push("Thank you for your order!");
//   }

//   return lines.join("\n");
// }
// generatePosReceiptFromCart(
//   cart: any[],
//   deals: any[],
//   discountPercent: number,
//   type: 'customer' | 'kitchen',
//   orderId: number
// ): string {

//   const lines: string[] = [];
//   const SEP = "==============================================";

//   // RESET PRINTER
//   lines.push('\x1B\x40');

//   // ===== HEADER =====
//   lines.push('\x1B\x61\x01'); // CENTER
//   lines.push('\x1B\x45\x01'); // BOLD
//   //lines.push('\x1D\x21\x11'); // DOUBLE SIZE
//   lines.push("DATA FINGER CHIPS");
//   lines.push('\x1B\x45\x00');
//   lines.push("0900-78601");
//   lines.push("");

//   lines.push(type === 'customer' ? "CUSTOMER COPY" : "KITCHEN COPY");
//   lines.push("");

//   // LEFT ALIGN
//   lines.push('\x1B\x61\x00');
//   lines.push(`Order ID: ${orderId}`);
//   lines.push(`Date: ${new Date().toLocaleString()}`);

//   lines.push(SEP);

//   // ===== ITEMS =====
//   cart.forEach(item => {
//     // FULL DESCRIPTION (wrap naturally)
//     lines.push(item.name);

//     const rate = item.unitPrice.toString().padEnd(8);
//     const qty  = item.quantity.toString().padEnd(8);
//     const val  = item.total.toString().padStart(8);

//     lines.push(
//       `Rate: ${rate} Qty: ${qty} Val: Rs ${val}`
//     );
//     lines.push("");
//   });

//   // ===== DEALS =====
//   deals.forEach(d => {
//     const total = d.dealPrice * d.quantity;

//     lines.push(d.dealName);

//     const rate = d.dealPrice.toString().padEnd(8);
//     const qty  = d.quantity.toString().padEnd(8);
//     const val  = total.toString().padStart(8);

//     lines.push(
//       `Rate: ${rate} Qty: ${qty} Val: Rs ${val}`
//     );
//     lines.push("");
//   });

//   lines.push(SEP);

//   // ===== TOTAL CALC =====
//   const itemsTotal = cart.reduce((a, b) => a + (b.total || 0), 0);
//   const dealsTotal = deals.reduce((a, d) => a + d.dealPrice * d.quantity, 0);
//   const discountAmount =
//     discountPercent > 0 ? (itemsTotal * discountPercent) / 100 : 0;

//   const finalTotal = itemsTotal - discountAmount + dealsTotal;

//   if (discountPercent > 0) {
//     lines.push(`Subtotal: Rs ${itemsTotal + dealsTotal}`);
//     lines.push(`Discount (${discountPercent}%): -Rs ${discountAmount}`);
//     lines.push(SEP);
//   }

//   // ===== BIG TOTAL =====
//   lines.push('\x1B\x45\x01');       // BOLD
//   lines.push('\x1D\x21\x11');       // DOUBLE SIZE
//   lines.push(`TOTAL: Rs ${finalTotal}`);
//   lines.push('\x1D\x21\x00');
//   lines.push('\x1B\x45\x00');

//   // ===== FOOTER =====
//   if (type === 'customer') {
//     lines.push("");
//     lines.push(SEP);
//     lines.push('\x1B\x61\x01');
//     lines.push("Thank you for your order!");
//     lines.push("Robionix Technologies");
//     lines.push("Cell: 0328-5107458,0303-5184773");
//     lines.push('\x1B\x61\x00');
//   }

//   // FEED + CUT
//   lines.push("\n\n\n\x1D\x56\x00");

//   return lines.join("\n");
// }

}

