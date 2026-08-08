import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DailySalaryReport } from './daily-salary-report';

describe('DailySalaryReport', () => {
  let component: DailySalaryReport;
  let fixture: ComponentFixture<DailySalaryReport>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DailySalaryReport]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DailySalaryReport);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
