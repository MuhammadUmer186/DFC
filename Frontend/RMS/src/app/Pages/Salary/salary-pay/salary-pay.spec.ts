import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SalaryPay } from './salary-pay';

describe('SalaryPay', () => {
  let component: SalaryPay;
  let fixture: ComponentFixture<SalaryPay>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SalaryPay]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SalaryPay);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
