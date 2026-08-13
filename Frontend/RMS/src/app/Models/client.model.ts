export interface Client {
  id: number;
  phoneNumber: string;
  name?: string | null;
  address?: string | null;
  createdAt: string;
  updatedAt?: string | null;
}
