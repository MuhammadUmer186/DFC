import { TestBed } from '@angular/core/testing';

import { OrderSignalr } from './order-signalr';

describe('OrderSignalr', () => {
  let service: OrderSignalr;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(OrderSignalr);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
