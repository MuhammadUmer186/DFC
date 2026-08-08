import { ComponentFixture, TestBed } from '@angular/core/testing';

import { KitchenorderComponent } from './kitchenorder.component';

describe('KitchenorderComponent', () => {
  let component: KitchenorderComponent;
  let fixture: ComponentFixture<KitchenorderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [KitchenorderComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(KitchenorderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
