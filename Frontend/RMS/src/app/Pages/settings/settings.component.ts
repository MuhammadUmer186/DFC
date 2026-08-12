import { Component, OnInit, signal } from '@angular/core';
import QRCode from 'qrcode';
import { ServiceTimeSetting } from '../../Models/service-time-setting.model';
import { ServiceTimeSettingService } from '../../Services/service-time-setting.service';
import { SiteSettingService } from '../../Services/site-setting.service';
import { BrandingService } from '../../Services/branding.service';
import { ToastService } from '../../Services/toast.service';
import { environment } from '../../../environments/environment';
import { COUNTRY_TIMEZONES } from '../../Shared/country-timezones';

const SERVICE_TYPE_LABELS: Record<string, string> = {
  DineIn: 'Dine-in',
  Takeaway: 'Takeaway',
  Delivery: 'Delivery'
};

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.css'
})
export class SettingsComponent implements OnInit {
  settings = signal<ServiceTimeSetting[]>([]);
  savingType = signal<string | null>(null);

  heroImageUrl = signal<string | null>(null);
  uploadingHero = signal(false);

  whatsAppNumber = signal('');
  savingWhatsApp = signal(false);

  restaurantName = signal('');
  savingRestaurantName = signal(false);
  logoUrl = signal<string | null>(null);
  uploadingLogo = signal(false);

  orderSerialPrefix = signal('');
  orderSerialStartingNumber = signal(1);
  orderSerialResetTime = signal('00:00');
  savingOrderSerial = signal(false);

  menuPdfUrl = signal<string | null>(null);
  uploadingMenuPdf = signal(false);
  menuQrTargetUrl = signal(this.defaultMenuQrTargetUrl());
  menuQrDataUrl = signal<string | null>(null);
  generatingQr = signal(false);

  countryTimeZones = COUNTRY_TIMEZONES;
  country = signal('');
  timeZoneId = signal('');
  currencyCode = signal('');
  currencySymbol = signal('');
  savingCountryTimeZone = signal(false);

  editValues = new Map<string, { min: number; max: number; enabled: boolean }>();

  constructor(
    private service: ServiceTimeSettingService,
    private siteSettingService: SiteSettingService,
    private branding: BrandingService,
    private toast: ToastService
  ) {}

  loadError = signal<string | null>(null);

  ngOnInit() {
    this.load();
    this.loadSiteSettings();
  }

  loadSiteSettings() {
    this.siteSettingService.get().subscribe({
      next: (res) => {
        this.heroImageUrl.set(res.heroImageUrl);
        this.whatsAppNumber.set(res.whatsAppNumber ?? '');
        this.restaurantName.set(res.restaurantName ?? '');
        this.logoUrl.set(res.logoUrl);
        this.orderSerialPrefix.set(res.orderSerialPrefix ?? '');
        this.orderSerialStartingNumber.set(res.orderSerialStartingNumber ?? 1);
        // Backend sends "HH:mm:ss" — <input type="time"> wants "HH:mm"
        this.orderSerialResetTime.set((res.orderSerialResetTime ?? '00:00:00').slice(0, 5));
        this.menuPdfUrl.set(res.menuPdfUrl);
        this.country.set(res.country ?? 'Pakistan');
        this.timeZoneId.set(res.timeZoneId ?? 'Asia/Karachi');
        this.currencyCode.set(res.currencyCode ?? 'PKR');
        this.currencySymbol.set(res.currencySymbol ?? 'Rs');
      },
      error: (err) => this.handleLoadError(err)
    });
  }

  // Selecting a country auto-fills its primary time zone and currency; the admin can still
  // fine-tune either afterwards (e.g. a country spanning multiple zones, or a symbol
  // preference) before saving.
  onCountrySelected(value: string) {
    this.country.set(value);
    const match = this.countryTimeZones.find(c => c.country === value);
    if (match) {
      this.timeZoneId.set(match.timeZoneId);
      this.currencyCode.set(match.currencyCode);
      this.currencySymbol.set(match.currencySymbol);
    }
  }

  setTimeZoneId(value: string) {
    this.timeZoneId.set(value);
  }

  setCurrencyCode(value: string) {
    this.currencyCode.set(value);
  }

  setCurrencySymbol(value: string) {
    this.currencySymbol.set(value);
  }

  saveCountryTimeZone() {
    if (!this.country().trim() || !this.timeZoneId().trim() || !this.currencyCode().trim() || !this.currencySymbol().trim()) {
      this.toast.error('Select a country, time zone and currency');
      return;
    }

    this.savingCountryTimeZone.set(true);
    this.siteSettingService.updateCountryTimeZone(
      this.country().trim(),
      this.timeZoneId().trim(),
      this.currencyCode().trim(),
      this.currencySymbol().trim()
    ).subscribe({
      next: (res) => {
        this.country.set(res.country);
        this.timeZoneId.set(res.timeZoneId);
        this.currencyCode.set(res.currencyCode);
        this.currencySymbol.set(res.currencySymbol);
        this.branding.country.set(res.country);
        this.branding.timeZoneId.set(res.timeZoneId);
        this.branding.currencyCode.set(res.currencyCode);
        this.branding.currencySymbol.set(res.currencySymbol);
        this.savingCountryTimeZone.set(false);
        this.toast.success('Country, time zone & currency updated');
      },
      error: (err) => {
        this.savingCountryTimeZone.set(false);
        this.toast.error(err?.error || 'Failed to update country, time zone & currency');
      }
    });
  }

  // Absolute URL of the stable Public/menu-pdf redirect — this is what the QR code encodes.
  // It never changes even if the uploaded PDF is replaced, so a printed QR stays valid forever
  // as long as this domain keeps pointing at the backend (survives VPS/server changes, not domain changes).
  private defaultMenuQrTargetUrl(): string {
    const base = environment.apiBaseUrl.startsWith('http')
      ? environment.apiBaseUrl
      : `${window.location.origin}${environment.apiBaseUrl}`;
    return `${base}/public/menu-pdf`;
  }

  fullMenuPdfUrl(): string | null {
    const url = this.menuPdfUrl();
    return url ? `${environment.apihub}${url}` : null;
  }

  onMenuPdfFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    const maxBytes = 20 * 1024 * 1024;
    if (file.size > maxBytes) {
      this.toast.error(`File is too large (${(file.size / 1024 / 1024).toFixed(1)} MB). Please use a file under 20 MB.`);
      input.value = '';
      return;
    }
    if (file.type !== 'application/pdf' && !file.name.toLowerCase().endsWith('.pdf')) {
      this.toast.error('Please upload a PDF file');
      input.value = '';
      return;
    }

    this.uploadingMenuPdf.set(true);
    this.siteSettingService.uploadMenuPdf(file).subscribe({
      next: (res) => {
        this.menuPdfUrl.set(res.menuPdfUrl);
        this.uploadingMenuPdf.set(false);
        this.toast.success('Menu PDF updated — the QR code (if already printed) still points to it automatically');
        input.value = '';
      },
      error: (err) => {
        this.uploadingMenuPdf.set(false);
        this.toast.error(this.describeUploadError(err));
        input.value = '';
      }
    });
  }

  setMenuQrTargetUrl(value: string) {
    this.menuQrTargetUrl.set(value);
  }

  async generateMenuQr() {
    const url = this.menuQrTargetUrl().trim();
    if (!url) {
      this.toast.error('Enter the URL the QR code should open');
      return;
    }

    this.generatingQr.set(true);
    try {
      const dataUrl = await QRCode.toDataURL(url, { width: 512, margin: 2 });
      this.menuQrDataUrl.set(dataUrl);
    } catch {
      this.toast.error('Failed to generate QR code');
    } finally {
      this.generatingQr.set(false);
    }
  }

  downloadMenuQr() {
    const dataUrl = this.menuQrDataUrl();
    if (!dataUrl) return;

    const link = document.createElement('a');
    link.href = dataUrl;
    link.download = 'menu-qr.png';
    link.click();
  }

  setOrderSerialPrefix(value: string) {
    this.orderSerialPrefix.set(value);
  }

  setOrderSerialStartingNumber(value: string) {
    this.orderSerialStartingNumber.set(Number(value));
  }

  setOrderSerialResetTime(value: string) {
    this.orderSerialResetTime.set(value);
  }

  saveOrderSerialSetting() {
    if (this.orderSerialStartingNumber() < 0) {
      this.toast.error('Starting number cannot be negative');
      return;
    }

    this.savingOrderSerial.set(true);
    const resetTimeSeconds = `${this.orderSerialResetTime()}:00`;
    this.siteSettingService.updateOrderSerialSetting(this.orderSerialPrefix().trim(), this.orderSerialStartingNumber(), resetTimeSeconds).subscribe({
      next: (res) => {
        this.orderSerialPrefix.set(res.orderSerialPrefix ?? '');
        this.orderSerialStartingNumber.set(res.orderSerialStartingNumber ?? 1);
        this.orderSerialResetTime.set((res.orderSerialResetTime ?? '00:00:00').slice(0, 5));
        this.savingOrderSerial.set(false);
        this.toast.success('Order serial setting updated');
      },
      error: (err) => {
        this.savingOrderSerial.set(false);
        this.toast.error(err?.error || 'Failed to update order serial setting');
      }
    });
  }

  setWhatsAppNumber(value: string) {
    this.whatsAppNumber.set(value);
  }

  saveWhatsAppNumber() {
    this.savingWhatsApp.set(true);
    this.siteSettingService.updateWhatsAppNumber(this.whatsAppNumber().trim()).subscribe({
      next: (res) => {
        this.whatsAppNumber.set(res.whatsAppNumber ?? '');
        this.savingWhatsApp.set(false);
        this.toast.success('WhatsApp number updated');
      },
      error: (err) => {
        this.savingWhatsApp.set(false);
        this.toast.error(err?.error || 'Failed to update WhatsApp number');
      }
    });
  }

  setRestaurantName(value: string) {
    this.restaurantName.set(value);
  }

  saveRestaurantName() {
    const name = this.restaurantName().trim();
    if (!name) {
      this.toast.error('Restaurant name is required');
      return;
    }
    this.savingRestaurantName.set(true);
    this.siteSettingService.updateRestaurantName(name).subscribe({
      next: (res) => {
        this.restaurantName.set(res.restaurantName ?? '');
        this.savingRestaurantName.set(false);
        this.toast.success('Restaurant name updated');
        this.branding.restaurantName.set(res.restaurantName ?? name);
      },
      error: (err) => {
        this.savingRestaurantName.set(false);
        this.toast.error(err?.error || 'Failed to update restaurant name');
      }
    });
  }

  fullLogoUrl(): string | null {
    const url = this.logoUrl();
    return url ? `${environment.apihub}${url}` : null;
  }

  onLogoFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    const maxBytes = 5 * 1024 * 1024;
    if (file.size > maxBytes) {
      this.toast.error(`Image is too large (${(file.size / 1024 / 1024).toFixed(1)} MB). Please use a file under 5 MB.`);
      input.value = '';
      return;
    }

    this.uploadingLogo.set(true);
    this.siteSettingService.uploadLogo(file).subscribe({
      next: (res) => {
        this.logoUrl.set(res.logoUrl);
        this.uploadingLogo.set(false);
        this.toast.success('Logo updated');
        this.branding.logoUrl.set(`${environment.apihub}${res.logoUrl}`);
        input.value = '';
      },
      error: (err) => {
        this.uploadingLogo.set(false);
        this.toast.error(this.describeUploadError(err));
        input.value = '';
      }
    });
  }

  private handleLoadError(err: any) {
    if (err?.status === 401 || err?.status === 403) {
      this.loadError.set('Your session has expired or you no longer have permission to view this page. Please log out and log back in.');
    } else if (err?.status === 0) {
      this.loadError.set("Couldn't reach the server — check that the backend is running.");
    } else {
      this.loadError.set('Failed to load settings. Please refresh the page.');
    }
  }

  fullHeroImageUrl(): string | null {
    const url = this.heroImageUrl();
    return url ? `${environment.apihub}${url}` : null;
  }

  onHeroFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    const maxBytes = 15 * 1024 * 1024;
    if (file.size > maxBytes) {
      this.toast.error(`Image is too large (${(file.size / 1024 / 1024).toFixed(1)} MB). Please use a file under 15 MB.`);
      input.value = '';
      return;
    }

    this.uploadingHero.set(true);
    this.siteSettingService.uploadHeroImage(file).subscribe({
      next: (res) => {
        this.heroImageUrl.set(res.heroImageUrl);
        this.uploadingHero.set(false);
        this.toast.success('Hero banner image updated');
        input.value = '';
      },
      error: (err) => {
        this.uploadingHero.set(false);
        this.toast.error(this.describeUploadError(err));
        input.value = '';
      }
    });
  }

  private describeUploadError(err: any): string {
    if (err?.status === 0) {
      return "Couldn't reach the server — check that the backend is running and reachable, then try again.";
    }
    if (err?.status === 401 || err?.status === 403) {
      return 'Your session has expired or you no longer have permission. Please log out and log back in, then try again.';
    }
    if (err?.status === 413) {
      return 'File is too large for the server to accept.';
    }
    if (typeof err?.error === 'string' && err.error) {
      return err.error;
    }
    return `Upload failed (status ${err?.status ?? 'unknown'}). Please try again or check with support.`;
  }

  load() {
    this.service.getAll().subscribe({
      next: (res) => {
        this.settings.set(res);
        for (const s of res) {
          this.editValues.set(s.serviceType, { min: s.minMinutes, max: s.maxMinutes, enabled: s.isEnabled });
        }
      },
      error: (err) => this.handleLoadError(err)
    });
  }

  label(serviceType: string): string {
    return SERVICE_TYPE_LABELS[serviceType] ?? serviceType;
  }

  getMin(serviceType: string): number {
    return this.editValues.get(serviceType)?.min ?? 0;
  }

  getMax(serviceType: string): number {
    return this.editValues.get(serviceType)?.max ?? 0;
  }

  setMin(serviceType: string, value: string) {
    const entry = this.editValues.get(serviceType) ?? { min: 0, max: 0, enabled: true };
    entry.min = Number(value);
    this.editValues.set(serviceType, entry);
  }

  setMax(serviceType: string, value: string) {
    const entry = this.editValues.get(serviceType) ?? { min: 0, max: 0, enabled: true };
    entry.max = Number(value);
    this.editValues.set(serviceType, entry);
  }

  getEnabled(serviceType: string): boolean {
    return this.editValues.get(serviceType)?.enabled ?? true;
  }

  setEnabled(serviceType: string, enabled: boolean) {
    const entry = this.editValues.get(serviceType) ?? { min: 0, max: 0, enabled: true };
    entry.enabled = enabled;
    this.editValues.set(serviceType, entry);
  }

  save(setting: ServiceTimeSetting) {
    const entry = this.editValues.get(setting.serviceType);
    if (!entry) return;

    if (entry.min < 0 || entry.max < entry.min) {
      this.toast.error('Max minutes must be greater than or equal to min minutes');
      return;
    }

    this.savingType.set(setting.serviceType);
    this.service.update(setting.serviceType, entry.min, entry.max, entry.enabled).subscribe({
      next: () => {
        this.toast.success(`${this.label(setting.serviceType)} estimated time updated`);
        this.savingType.set(null);
        this.load();
      },
      error: (err) => {
        this.toast.error(err?.error || 'Failed to update estimated time');
        this.savingType.set(null);
      }
    });
  }
}
