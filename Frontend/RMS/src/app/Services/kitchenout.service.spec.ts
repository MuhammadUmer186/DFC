import { TestBed } from '@angular/core/testing';

import { KitchenoutService } from './kitchenout.service';

describe('KitchenoutService', () => {
  let service: KitchenoutService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(KitchenoutService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
