import { TestBed } from '@angular/core/testing';

import { Printservice } from './printservice';

describe('Printservice', () => {
  let service: Printservice;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Printservice);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
