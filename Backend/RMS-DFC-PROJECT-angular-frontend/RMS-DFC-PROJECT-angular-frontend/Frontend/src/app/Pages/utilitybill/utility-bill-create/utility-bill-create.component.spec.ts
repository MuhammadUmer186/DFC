import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UtilityBillCreateComponent } from './utility-bill-create.component';

describe('UtilityBillCreateComponent', () => {
  let component: UtilityBillCreateComponent;
  let fixture: ComponentFixture<UtilityBillCreateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UtilityBillCreateComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UtilityBillCreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
