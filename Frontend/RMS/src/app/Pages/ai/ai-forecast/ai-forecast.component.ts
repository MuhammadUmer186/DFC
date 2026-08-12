import { Component, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AiForecastService, ForecastRun, ForecastValue } from '../../../Services/ai-forecast.service';
import { ToastService } from '../../../Services/toast.service';
import { RmsCurrencyPipe } from '../../../Shared/pipes/currency-symbol.pipe';

@Component({
  selector: 'app-ai-forecast',
  standalone: true,
  imports: [CommonModule, FormsModule, RmsCurrencyPipe],
  templateUrl: './ai-forecast.component.html',
  styleUrl: './ai-forecast.component.css'
})
export class AiForecastComponent {
  run = signal<ForecastRun | null>(null);
  loading = signal(false);
  recalculating = signal(false);
  loadError = signal<string | null>(null);
  horizonDays = signal(7);

  // Day-level rows: one per forecasted date (HourOfDay == null && MenuItemId == null)
  dayRows = computed(() => (this.run()?.values ?? []).filter(v => v.hourOfDay === null && v.menuItemId === null));

  // Item-level rows for the first forecasted date, sorted by predicted quantity — a simple
  // "what to prepare first" view rather than a picker for every date in the horizon.
  topItemsFirstDay = computed(() => {
    const rows = this.dayRows();
    if (rows.length === 0) return [] as ForecastValue[];
    const firstDate = rows[0].forecastDate;
    return (this.run()?.values ?? [])
      .filter(v => v.forecastDate === firstDate && v.menuItemId !== null)
      .sort((a, b) => b.predictedQuantity - a.predictedQuantity)
      .slice(0, 15);
  });

  // Hourly rows for the first forecasted date — "peak periods."
  hourlyFirstDay = computed(() => {
    const rows = this.dayRows();
    if (rows.length === 0) return [] as ForecastValue[];
    const firstDate = rows[0].forecastDate;
    return (this.run()?.values ?? [])
      .filter(v => v.forecastDate === firstDate && v.hourOfDay !== null)
      .sort((a, b) => (a.hourOfDay ?? 0) - (b.hourOfDay ?? 0));
  });

  maxHourlySales = computed(() => Math.max(1, ...this.hourlyFirstDay().map(h => h.predictedSales)));

  constructor(private service: AiForecastService, private toast: ToastService) {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.loadError.set(null);
    this.service.getLatest().subscribe({
      next: (run) => {
        this.run.set(run);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        if (err?.status === 404) {
          this.run.set(null); // no forecast generated yet — not an error state
        } else {
          this.loadError.set(err?.error?.message || 'Failed to load forecast');
        }
      }
    });
  }

  recalculate() {
    this.recalculating.set(true);
    this.service.recalculate(this.horizonDays()).subscribe({
      next: (run) => {
        this.run.set(run);
        this.recalculating.set(false);
        this.toast.success('Forecast recalculated');
      },
      error: (err) => {
        this.recalculating.set(false);
        this.toast.error(err?.error?.message || 'Failed to recalculate forecast');
      }
    });
  }

  setHorizon(value: string) {
    const n = Number(value);
    if (n >= 1 && n <= 30) this.horizonDays.set(n);
  }

  hourLabel(hour: number | null): string {
    if (hour === null) return '';
    return `${hour.toString().padStart(2, '0')}:00`;
  }
}
