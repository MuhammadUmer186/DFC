import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WastemanagementComponent } from './wastemanagement.component';

describe('WastemanagementComponent', () => {
  let component: WastemanagementComponent;
  let fixture: ComponentFixture<WastemanagementComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WastemanagementComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WastemanagementComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
