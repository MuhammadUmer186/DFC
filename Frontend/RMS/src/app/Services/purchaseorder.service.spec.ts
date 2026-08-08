import { TestBed } from '@angular/core/testing';

import { PurchaseorderService } from './purchaseorder.service';

describe('PurchaseorderService', () => {
  let service: PurchaseorderService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PurchaseorderService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
