import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VendorAccountComponent } from './vendor-account.component';

describe('VendorAccountComponent', () => {
  let component: VendorAccountComponent;
  let fixture: ComponentFixture<VendorAccountComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VendorAccountComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VendorAccountComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
