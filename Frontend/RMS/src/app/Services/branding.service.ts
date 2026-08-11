import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Title } from '@angular/platform-browser';
import { environment } from '../../environments/environment';

// Public, unauthenticated — safe to call before login (sidebar/navbar/login page all need
// the restaurant's name/logo before there's any token). Backed by the same SiteSettings
// row the super admin edits on the Site Settings page, so white-labeling for a different
// restaurant only requires changing that one place.
@Injectable({ providedIn: 'root' })
export class BrandingService {
  private api = environment.apiBaseUrl + '/Public/site-settings';
  private loaded = false;

  restaurantName = signal<string>('DFC');
  logoUrl = signal<string | null>(null);

  constructor(private http: HttpClient, private titleService: Title) {}

  load() {
    if (this.loaded) return;
    this.loaded = true;
    this.http.get<{ restaurantName: string | null; logoUrl: string | null }>(this.api).subscribe({
      next: (res) => {
        if (res.restaurantName) {
          this.restaurantName.set(res.restaurantName);
          this.titleService.setTitle(`${res.restaurantName} — RMS`);
        }
        if (res.logoUrl) this.logoUrl.set(`${environment.apihub}${res.logoUrl}`);
      },
      error: () => {
        // Non-critical — falls back to the default name/logo baked into each template.
      }
    });
  }
}
