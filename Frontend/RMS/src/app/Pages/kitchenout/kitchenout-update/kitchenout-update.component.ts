import { Component, OnInit } from '@angular/core';

import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { KitchenOutService } from '../../../Services/kitchenout.service';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './kitchenout-update.component.html'
})
export class KitchenOutUpdateComponent implements OnInit {

  form!: FormGroup;
  id!: number;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private service: KitchenOutService,
    private router: Router
  ) {
    this.form = this.fb.group({
      itemName: ['', Validators.required],
      quantity: [0, Validators.required],
      unit: ['', Validators.required],
      date: ['', Validators.required]
    });
  }

  ngOnInit() {
    this.id = Number(this.route.snapshot.paramMap.get('id'));

    this.service.getAll().subscribe(res => {
      const record = res.find(x => x.id === this.id);
      if (record) {
        this.form.patchValue(record);
      }
    });
  }

  update() {
    if (this.form.invalid) return;

    this.service.update(this.id, this.form.value)
      .subscribe(() => this.router.navigate(['/kitchenout']));
  }
}
