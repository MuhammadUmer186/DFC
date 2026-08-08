import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { EmployeeService } from '../../../Services/employee';

@Component({
  selector: 'app-employee-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './employee-list.html',
  styleUrls: ['./employee-list.css']
})
export class EmployeeListComponent {

  employee = signal<any[]>([]);

  constructor(public service: EmployeeService) {
    this.load();
  }

  load() {
    this.service.loadAll().subscribe({
      next: (res) => {
        this.employee.set(res);
      },
      error: (err) => {
        console.log(err);
      }
    });
  }

  delete(id: number) {

    if (!confirm('Are you sure you want to delete this employee?'))
      return;

    this.service.delete(id).subscribe({
      next: () => {

        this.employee.update(list =>
          list.filter(x => x.id !== id)
        );

      },
      error: (err) => {
        console.log(err);
      }
    });
  }
}