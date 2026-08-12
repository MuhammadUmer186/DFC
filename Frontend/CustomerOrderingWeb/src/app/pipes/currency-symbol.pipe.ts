import { Pipe, PipeTransform, inject } from '@angular/core';
import { formatNumber } from '@angular/common';
import { SiteSettingService } from '../services/site-setting.service';

// Replaces hardcoded "Rs {{ amount }}" template text so every price follows the currency
// configured in the RMS (Settings → Country, Time Zone & Currency). Impure — the symbol can
// change at runtime after SiteSettingService.load() resolves.
// Usage: {{ amount | rmsCurrency }}  or  {{ amount | rmsCurrency:'1.0-2' }} (same digitsInfo the
// built-in `number` pipe takes) to match a previous "Rs {{ amount | number:'1.0-2' }}" call site.
@Pipe({ name: 'rmsCurrency', standalone: true, pure: false })
export class RmsCurrencyPipe implements PipeTransform {
  private siteSettings = inject(SiteSettingService);

  transform(value: number | string | null | undefined, digitsInfo?: string): string {
    const symbol = this.siteSettings.currencySymbol();

    if (value === null || value === undefined || value === '') {
      return `${symbol} 0`;
    }

    if (digitsInfo) {
      const num = typeof value === 'string' ? parseFloat(value) : value;
      if (!Number.isNaN(num)) {
        return `${symbol} ${formatNumber(num, 'en-US', digitsInfo)}`;
      }
    }

    return `${symbol} ${value}`;
  }
}
