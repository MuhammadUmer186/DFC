import { ComponentFixture, TestBed } from '@angular/core/testing';

import { KitchenoutdataComponent } from './kitchenoutdata.component';

describe('KitchenoutdataComponent', () => {
  let component: KitchenoutdataComponent;
  let fixture: ComponentFixture<KitchenoutdataComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [KitchenoutdataComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(KitchenoutdataComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
