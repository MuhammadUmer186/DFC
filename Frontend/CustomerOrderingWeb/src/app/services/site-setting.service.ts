import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class SiteSettingService {
  private baseUrl = environment.apiBaseUrl + '/Public';

  private heroImageUrlRaw = signal<string | null>(null);
  whatsAppNumber = signal<string | null>(null);

  heroImageUrl = computed(() => {
    const url = this.heroImageUrlRaw();
    return url ? `${environment.apihub}${url}` : null;
  });

  constructor(private http: HttpClient) {
    this.load();
  }

  load() {
    this.http.get<{ heroImageUrl: string | null; whatsAppNumber: string | null }>(`${this.baseUrl}/site-settings`).subscribe({
      next: (res) => {
        this.heroImageUrlRaw.set(res.heroImageUrl);
        this.whatsAppNumber.set(res.whatsAppNumber);
      },
      error: () => {}
    });
  }
}
