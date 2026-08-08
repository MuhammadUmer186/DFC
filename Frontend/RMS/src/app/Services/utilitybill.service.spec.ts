import { TestBed } from '@angular/core/testing';

import { UtilitybillService } from './utilitybill.service';

describe('UtilitybillService', () => {
  let service: UtilitybillService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(UtilitybillService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
