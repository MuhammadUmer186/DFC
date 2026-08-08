export interface Rider {
  id: number;
  name: string;
  phoneNumber: string;
  vehicleNumber?: string | null;
  isActive: boolean;
  createdAt: string;
  activeDeliveryCount: number;
}
