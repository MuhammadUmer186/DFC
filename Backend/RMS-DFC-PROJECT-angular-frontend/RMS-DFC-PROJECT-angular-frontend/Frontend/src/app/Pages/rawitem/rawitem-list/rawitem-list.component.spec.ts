import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RawitemListComponent } from './rawitem-list.component';

describe('RawitemListComponent', () => {
  let component: RawitemListComponent;
  let fixture: ComponentFixture<RawitemListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RawitemListComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RawitemListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
