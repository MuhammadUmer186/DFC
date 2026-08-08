import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SalaryService } from '../../../Services/salary';


@Component({
  selector: 'app-daily-salary-report',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './daily-salary-report.html',
  styleUrls: ['./daily-salary-report.css']
})
export class DailySalaryReportComponent {
  date!: string;

  constructor(public service: SalaryService) {}


}
