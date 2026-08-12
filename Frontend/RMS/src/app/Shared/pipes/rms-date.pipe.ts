import { Pipe, PipeTransform, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { BrandingService } from '../../Services/branding.service';

// Wraps Angular's built-in `date` pipe, pinning the display to the restaurant's configured time
// zone (Settings → Country, Time Zone & Currency) instead of the viewing device's own OS/browser
// zone. Backend timestamps are correctly UTC-tagged (see Backend/Data/UtcDateTimeConverters.cs),
// so without this a device with a different system time zone would silently show the wrong time.
// Usage: {{ order.createdAt | rmsDate:'medium' }} — same format strings as the `date` pipe.
@Pipe({ name: 'rmsDate', standalone: true, pure: false })
export class RmsDatePipe implements PipeTransform {
  private branding = inject(BrandingService);
  private datePipe = new DatePipe('en-US');

  transform(value: string | number | Date | null | undefined, format = 'medium'): string | null {
    if (value === null || value === undefined || value === '') return null;
    return this.datePipe.transform(value, format, this.branding.timeZoneId());
  }
}
