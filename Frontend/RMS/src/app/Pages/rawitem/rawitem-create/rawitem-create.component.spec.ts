import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RawitemCreateComponent } from './rawitem-create.component';

describe('RawitemCreateComponent', () => {
  let component: RawitemCreateComponent;
  let fixture: ComponentFixture<RawitemCreateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RawitemCreateComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RawitemCreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
