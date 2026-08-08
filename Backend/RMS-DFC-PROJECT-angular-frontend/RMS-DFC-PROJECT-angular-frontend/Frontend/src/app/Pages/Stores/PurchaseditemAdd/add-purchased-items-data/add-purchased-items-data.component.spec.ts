import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddPurchasedItemsDataComponent } from './add-purchased-items-data.component';

describe('AddPurchasedItemsDataComponent', () => {
  let component: AddPurchasedItemsDataComponent;
  let fixture: ComponentFixture<AddPurchasedItemsDataComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddPurchasedItemsDataComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddPurchasedItemsDataComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
