import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Report1 } from './report1';

describe('Report1', () => {
  let component: Report1;
  let fixture: ComponentFixture<Report1>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Report1]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Report1);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
