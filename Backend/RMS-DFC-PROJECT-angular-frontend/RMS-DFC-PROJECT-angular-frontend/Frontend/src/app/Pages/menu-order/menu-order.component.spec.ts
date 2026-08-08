import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MenuOrderComponent } from './menu-order.component';

describe('MenuOrderComponent', () => {
  let component: MenuOrderComponent;
  let fixture: ComponentFixture<MenuOrderComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MenuOrderComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MenuOrderComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
