import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BrandingService } from '../../Services/branding.service';

// Displays the restaurant's local date/time, ticking every second. The time zone comes from
// Settings → Country & Time Zone (BrandingService), so the clock always shows the restaurant's
// local time regardless of which device/location is viewing the dashboard.
@Component({
  selector: 'app-live-clock',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './live-clock.component.html',
  styleUrls: ['./live-clock.component.css']
})
export class LiveClockComponent implements OnInit, OnDestroy {
  timeText = signal('');
  dateText = signal('');

  private intervalId?: ReturnType<typeof setInterval>;

  constructor(private branding: BrandingService) {
    this.branding.load();
  }

  ngOnInit() {
    this.tick();
    this.intervalId = setInterval(() => this.tick(), 1000);
  }

  ngOnDestroy() {
    if (this.intervalId) clearInterval(this.intervalId);
  }

  private tick() {
    const timeZone = this.branding.timeZoneId();
    const now = new Date();

    this.timeText.set(
      new Intl.DateTimeFormat('en-US', {
        timeZone,
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit',
        hour12: true
      }).format(now)
    );

    this.dateText.set(
      new Intl.DateTimeFormat('en-US', {
        timeZone,
        weekday: 'short',
        day: '2-digit',
        month: 'short',
        year: 'numeric'
      }).format(now)
    );
  }
}
