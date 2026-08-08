import { MenuItem } from './menu-item.model';

export interface Category {
  id: number;
  name: string;
  items: MenuItem[];
  imageUrl?: string | null;
}