import { ComponentFixture, TestBed } from '@angular/core/testing';

import { KitchenoutUpdateComponent } from './kitchenout-update.component';

describe('KitchenoutUpdateComponent', () => {
  let component: KitchenoutUpdateComponent;
  let fixture: ComponentFixture<KitchenoutUpdateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [KitchenoutUpdateComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(KitchenoutUpdateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
