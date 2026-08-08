export interface PublicMenuItem {
  id: number;
  name: string;
  price: number;
  imageUrl?: string | null;
  description?: string | null;
}

export interface PublicCategory {
  id: number;
  name: string;
  items: PublicMenuItem[];
}

export interface PublicDealItem {
  menuItemId: number;
  name: string;
  quantity: number;
}

export interface PublicDeal {
  id: number;
  dealName: string;
  price: number;
  imageUrl?: string | null;
  items: PublicDealItem[];
}

export interface PublicMenu {
  categories: PublicCategory[];
  deals: PublicDeal[];
}
