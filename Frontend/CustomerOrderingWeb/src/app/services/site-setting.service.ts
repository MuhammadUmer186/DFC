import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class SiteSettingService {
  private baseUrl = environment.apiBaseUrl + '/Public';

  private heroImageUrlRaw = signal<string | null>(null);
  private logoUrlRaw = signal<string | null>(null);
  whatsAppNumber = signal<string | null>(null);
  restaurantName = signal<string>('Data Finger Chips');
  // Admin-selected currency (RMS Settings → Country, Time Zone & Currency) — every price shown
  // on the ordering site reads this instead of a hardcoded symbol.
  currencySymbol = signal<string>('Rs');

  heroImageUrl = computed(() => {
    const url = this.heroImageUrlRaw();
    return url ? `${environment.apihub}${url}` : null;
  });

  logoUrl = computed(() => {
    const url = this.logoUrlRaw();
    return url ? `${environment.apihub}${url}` : null;
  });

  constructor(private http: HttpClient) {
    this.load();
  }

  load() {
    this.http.get<{ heroImageUrl: string | null; whatsAppNumber: string | null; restaurantName: string | null; logoUrl: string | null; currencySymbol: string | null }>(`${this.baseUrl}/site-settings`).subscribe({
      next: (res) => {
        this.heroImageUrlRaw.set(res.heroImageUrl);
        this.whatsAppNumber.set(res.whatsAppNumber);
        if (res.restaurantName) this.restaurantName.set(res.restaurantName);
        this.logoUrlRaw.set(res.logoUrl);
        if (res.currencySymbol) this.currencySymbol.set(res.currencySymbol);
      },
      error: () => {}
    });
  }
}
