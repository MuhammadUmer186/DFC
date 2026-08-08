import { Component, OnInit, signal } from '@angular/core';
import { TableModule } from 'primeng/table';
import { FormsModule } from '@angular/forms';
import { PurchaseOrderService } from '../../Services/purchaseorder.service';
import { ButtonModule } from 'primeng/button';

@Component({
  standalone: true,
  imports: [FormsModule],
  templateUrl: './stock-summary.component.html',
  styleUrls: ['./stock-summary.component.css']
})
export class StockSummaryComponent{

  stock=signal<any[]>([]);
  filteredStock=signal<any[]>([]);
  loading=signal<boolean>(true);

  searchText: string = "";

  constructor(private stockService: PurchaseOrderService) {
    this.stockService.getStockSummary()
      .subscribe(res => {
        this.stock.set(res);
        this.filteredStock.set(res);   // Initial load
        this.loading.set(false);
      });
  }

  

  ngDoCheck() {
    this.applyFilter();
  }

  applyFilter() {
  const text = this.searchText.toLowerCase();
  
  // update the filteredStock signal
  this.filteredStock.set(
    this.stock().filter(x =>
      x.itemName.toLowerCase().includes(text)
    )
  );
}

}
