import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MenuRecipe } from './menu-recipe';

describe('MenuRecipe', () => {
  let component: MenuRecipe;
  let fixture: ComponentFixture<MenuRecipe>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MenuRecipe]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MenuRecipe);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
