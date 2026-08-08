import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MenuItemReport } from './menu-item-report';

describe('MenuItemReport', () => {
  let component: MenuItemReport;
  let fixture: ComponentFixture<MenuItemReport>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MenuItemReport]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MenuItemReport);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
