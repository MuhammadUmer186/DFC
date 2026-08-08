export interface MenuItem {
  id: number;
  name: string;
  price: number;
  categoryId: number;
  categoryName?: string;
  imageUrl?: string | null;
  description?: string | null;
}
