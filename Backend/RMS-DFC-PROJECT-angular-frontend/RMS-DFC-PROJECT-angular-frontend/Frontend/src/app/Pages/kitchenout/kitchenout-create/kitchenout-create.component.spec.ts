import { ComponentFixture, TestBed } from '@angular/core/testing';

import { KitchenoutCreateComponent } from './kitchenout-create.component';

describe('KitchenoutCreateComponent', () => {
  let component: KitchenoutCreateComponent;
  let fixture: ComponentFixture<KitchenoutCreateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [KitchenoutCreateComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(KitchenoutCreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
