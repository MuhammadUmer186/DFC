import { ComponentFixture, TestBed } from '@angular/core/testing';

import { KitchenoutMaincreateComponent } from './kitchenout-maincreate.component';

describe('KitchenoutMaincreateComponent', () => {
  let component: KitchenoutMaincreateComponent;
  let fixture: ComponentFixture<KitchenoutMaincreateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [KitchenoutMaincreateComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(KitchenoutMaincreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
