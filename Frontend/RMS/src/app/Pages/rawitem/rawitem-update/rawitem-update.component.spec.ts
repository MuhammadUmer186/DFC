import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RawitemUpdateComponent } from './rawitem-update.component';

describe('RawitemUpdateComponent', () => {
  let component: RawitemUpdateComponent;
  let fixture: ComponentFixture<RawitemUpdateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RawitemUpdateComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RawitemUpdateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
