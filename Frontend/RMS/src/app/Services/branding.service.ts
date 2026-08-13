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
  // Company (parent/legal) branding — shown on the RMS top bar and login page, separate
  // from the restaurant brand above (sidebar, customer site).
  companyName = signal<string>('DFC');
  companyLogoUrl = signal<string | null>(null);
  // Admin-selected IANA time zone (Settings → Country & Time Zone) — drives the dashboard
  // clock so it shows the restaurant's local time regardless of the viewer's device/location.
  timeZoneId = signal<string>('Asia/Karachi');
  country = signal<string>('Pakistan');
  // Admin-selected currency (Settings → Country & Time Zone) — every "Rs {{ amount }}"-style
  // display in the RMS reads this instead of hardcoding a symbol, so switching the configured
  // country updates prices everywhere without touching individual pages.
  currencyCode = signal<string>('PKR');
  currencySymbol = signal<string>('Rs');

  constructor(private http: HttpClient, private titleService: Title) {}

  load() {
    if (this.loaded) return;
    this.loaded = true;
    this.http.get<{ restaurantName: string | null; logoUrl: string | null; companyName: string | null; companyLogoUrl: string | null; country: string | null; timeZoneId: string | null; currencyCode: string | null; currencySymbol: string | null }>(this.api).subscribe({
      next: (res) => {
        if (res.restaurantName) {
          this.restaurantName.set(res.restaurantName);
          this.titleService.setTitle(`${res.restaurantName} — RMS`);
        }
        if (res.logoUrl) this.logoUrl.set(`${environment.apihub}${res.logoUrl}`);
        if (res.companyName) this.companyName.set(res.companyName);
        if (res.companyLogoUrl) this.companyLogoUrl.set(`${environment.apihub}${res.companyLogoUrl}`);
        if (res.country) this.country.set(res.country);
        if (res.timeZoneId) this.timeZoneId.set(res.timeZoneId);
        if (res.currencyCode) this.currencyCode.set(res.currencyCode);
        if (res.currencySymbol) this.currencySymbol.set(res.currencySymbol);
      },
      error: () => {
        // Non-critical — falls back to the default name/logo baked into each template.
      }
    });
  }
}
