import { TestBed } from '@angular/core/testing';

import { RawitemService } from './rawitem.service';

describe('RawitemService', () => {
  let service: RawitemService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RawitemService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
