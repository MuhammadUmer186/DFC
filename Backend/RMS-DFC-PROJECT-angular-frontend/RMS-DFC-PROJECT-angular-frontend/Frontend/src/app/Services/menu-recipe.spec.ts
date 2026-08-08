import { TestBed } from '@angular/core/testing';

import { MenuRecipe } from './menu-recipe';

describe('MenuRecipe', () => {
  let service: MenuRecipe;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MenuRecipe);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
