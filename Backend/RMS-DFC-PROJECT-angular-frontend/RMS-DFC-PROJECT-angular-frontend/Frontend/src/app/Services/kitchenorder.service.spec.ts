import { TestBed } from '@angular/core/testing';

import { KitchenorderService } from './kitchenorder.service';

describe('KitchenorderService', () => {
  let service: KitchenorderService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(KitchenorderService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
