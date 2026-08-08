import { TestBed } from '@angular/core/testing';

import { VendorAccountService } from './vendor-account.service';

describe('VendorAccountService', () => {
  let service: VendorAccountService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(VendorAccountService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
