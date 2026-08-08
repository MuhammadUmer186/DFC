import { Component, OnInit, signal } from '@angular/core';
import { ServiceTimeSetting } from '../../../Models/service-time-setting.model';
import { ServiceTimeSettingService } from '../../../Services/service-time-setting.service';
import { SiteSettingService } from '../../../Services/site-setting.service';
import { ToastService } from '../../../Services/toast.service';
import { environment } from '../../../../environments/environment';

const SERVICE_TYPE_LABELS: Record<string, string> = {
  DineIn: 'Dine-in',
  Takeaway: 'Takeaway',
  Delivery: 'Delivery'
};

@Component({
  selector: 'app-service-time-settings',
  standalone: true,
  imports: [],
  templateUrl: './service-time-settings.component.html',
  styleUrl: './service-time-settings.component.css'
})
export class ServiceTimeSettingsComponent implements OnInit {
  settings = signal<ServiceTimeSetting[]>([]);
  savingType = signal<string | null>(null);

  heroImageUrl = signal<string | null>(null);
  uploadingHero = signal(false);

  whatsAppNumber = signal('');
  savingWhatsApp = signal(false);

  editValues = new Map<string, { min: number; max: number; enabled: boolean }>();

  constructor(
    private service: ServiceTimeSettingService,
    private siteSettingService: SiteSettingService,
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
      },
      error: (err) => this.handleLoadError(err)
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
      return 'Image is too large for the server to accept. Please use a file under 15 MB.';
    }
    if (typeof err?.error === 'string' && err.error) {
      return err.error;
    }
    return `Failed to upload hero banner image (status ${err?.status ?? 'unknown'}). Please try again or check with support.`;
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
