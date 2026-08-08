export interface ServiceTimeSetting {
  id: number;
  serviceType: 'DineIn' | 'Takeaway' | 'Delivery';
  minMinutes: number;
  maxMinutes: number;
  isEnabled: boolean;
}
