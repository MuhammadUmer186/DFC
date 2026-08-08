import { ComponentFixture, TestBed } from '@angular/core/testing';

import { KitchenoutListComponent } from './kitchenout-list.component';

describe('KitchenoutListComponent', () => {
  let component: KitchenoutListComponent;
  let fixture: ComponentFixture<KitchenoutListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [KitchenoutListComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(KitchenoutListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
