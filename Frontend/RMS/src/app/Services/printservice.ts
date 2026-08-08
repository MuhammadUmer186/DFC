import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
export interface PrintItem {
  name: string;
  quantity: number;
  price: number;
}

export interface PrintReceiptRequest  {
  copyType: 'customer' | 'kitchen';
  orderNo: number | string;
  total: number;
  discount: number;
  finalTotal: number;
  items: PrintItem[];
}
export type PrinterType =
  | 'usb1'
  | 'usb2'
  | 'ethernet'
  | 'bluetooth';

@Injectable({
  providedIn: 'root'
})
export class PrintService {
private baseUrll = environment.apiBaseUrl;
  private baseUrl = this.baseUrll + '/print';

  constructor(private http: HttpClient) {}

   /**
  @param request PrintReceiptRequest
   * @param printerType 'usb' or 'ethernet'
   */
  printReceipt(
    request: PrintReceiptRequest,
    printerType: PrinterType = 'usb1'
  ): Observable<any> {

    return this.http.post(
      `${this.baseUrl}/receipt?printer=${printerType}`,
      request
    );
  }
}
