import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PurchaseorderMaincreateComponent } from './purchaseorder-maincreate.component';

describe('PurchaseorderMaincreateComponent', () => {
  let component: PurchaseorderMaincreateComponent;
  let fixture: ComponentFixture<PurchaseorderMaincreateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PurchaseorderMaincreateComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PurchaseorderMaincreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
