export interface CartLine {
  kind: 'item' | 'deal';
  id: number; // menuItemId or dealId
  name: string;
  price: number;
  quantity: number;
  imageUrl?: string | null;
}
