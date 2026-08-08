import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface DateRange {
  from: string; // yyyy-MM-dd
  to: string;   // yyyy-MM-dd
  preset: string;
}

interface PresetOption {
  key: string;
  label: string;
}

@Component({
  selector: 'app-date-range-filter',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './date-range-filter.component.html',
  styleUrls: ['./date-range-filter.component.css']
})
export class DateRangeFilterComponent implements OnInit {

  /** Which preset is selected by default when the component loads */
  @Input() initialPreset: string = 'today';

  /** Emits whenever the effective range changes */
  @Output() rangeChange = new EventEmitter<DateRange>();

  presets: PresetOption[] = [
    { key: 'today', label: 'Today' },
    { key: 'yesterday', label: 'Yesterday' },
    { key: 'thisWeek', label: 'This Week' },
    { key: 'thisMonth', label: 'This Month' },
    { key: 'thisYear', label: 'This Year' },
    { key: 'overall', label: 'Overall' }
  ];

  activePreset: string = 'today';
  customFrom: string = '';
  customTo: string = '';

  ngOnInit(): void {
    this.selectPreset(this.initialPreset);
  }

  private toIso(date: Date): string {
    return date.toLocaleDateString('en-CA'); // yyyy-MM-dd, local time
  }

  private computeRange(preset: string): { from: string; to: string } {
    const today = new Date();
    const to = this.toIso(today);

    switch (preset) {
      case 'today':
        return { from: to, to };

      case 'yesterday': {
        const y = new Date(today);
        y.setDate(today.getDate() - 1);
        const yIso = this.toIso(y);
        return { from: yIso, to: yIso };
      }

      case 'thisWeek': {
        const day = today.getDay(); // 0 = Sunday
        const diffToMonday = day === 0 ? -6 : 1 - day;
        const monday = new Date(today);
        monday.setDate(today.getDate() + diffToMonday);
        return { from: this.toIso(monday), to };
      }

      case 'thisMonth': {
        const first = new Date(today.getFullYear(), today.getMonth(), 1);
        return { from: this.toIso(first), to };
      }

      case 'thisYear': {
        const first = new Date(today.getFullYear(), 0, 1);
        return { from: this.toIso(first), to };
      }

      case 'overall':
        return { from: '2000-01-01', to };

      default:
        return { from: to, to };
    }
  }

  selectPreset(preset: string): void {
    this.activePreset = preset;
    const { from, to } = this.computeRange(preset);
    this.customFrom = from;
    this.customTo = to;
    this.rangeChange.emit({ from, to, preset });
  }

  applyCustomRange(): void {
    if (!this.customFrom || !this.customTo) return;
    if (this.customTo < this.customFrom) return;

    this.activePreset = 'custom';
    this.rangeChange.emit({ from: this.customFrom, to: this.customTo, preset: 'custom' });
  }
}
