import { Component } from '@angular/core';

import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { RawItem, RawItemService } from '../../../Services/rawitem.service';
import { ToastService } from '../../../Services/toast.service';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './rawitem-update.component.html'
})
export class RawItemUpdateComponent {

  rawItem!: RawItem;

  itemName = new FormControl('', Validators.required);
  unit = new FormControl('', Validators.required);
  openingStock = new FormControl(0, Validators.required);

  constructor(private service: RawItemService, private router: Router, private toast: ToastService) {

    const data = history.state;

    if (!data || !data.id) {
      this.toast.warn("Invalid Access");
      this.router.navigate(['/rawitem']);
      return;
    }

    this.rawItem = data;

    this.itemName.setValue(this.rawItem.itemName);
    this.unit.setValue(this.rawItem.unit);
  }

  update() {
    if (this.itemName.invalid || this.unit.invalid) return;

    const updated: RawItem = {
      id: this.rawItem.id,
      itemName: this.itemName.value!,
      unit: this.unit.value!
    };

    this.service.update(this.rawItem.id, updated)
      .subscribe(() => this.router.navigate(['/rawitem']));
  }
}
