import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UtilityBillSearchComponent } from './utility-bill-search.component';

describe('UtilityBillSearchComponent', () => {
  let component: UtilityBillSearchComponent;
  let fixture: ComponentFixture<UtilityBillSearchComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UtilityBillSearchComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UtilityBillSearchComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
