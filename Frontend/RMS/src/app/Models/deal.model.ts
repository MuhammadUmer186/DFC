export interface DealItemInput {
  menuItemId: number;
  quantity: number;
}

export interface DealMenuItem {
  menuItemId: number;
  name: string;
  price: number;
  quantity: number;
}

export interface Deal {
  id: number;
  dealName: string;
  originalPrice: number;
  discountAmount: number;
  finalPrice: number;
  imageUrl?: string | null;
  isActive: boolean;
  items: DealMenuItem[];
}

export interface CreateDealPayload {
  dealName: string;
  discountAmount: number;
  items: DealItemInput[];
}
