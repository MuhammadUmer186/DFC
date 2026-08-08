import { Component } from '@angular/core';
import { FormGroup, FormArray, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';

import { RouterOutlet } from '@angular/router';
import { ToastService } from '../../../../Services/toast.service';


@Component({
  selector: 'app-add-purchased-items-data',
  standalone: true,
  imports: [ReactiveFormsModule, RouterOutlet],
  templateUrl: './add-purchased-items-data.component.html',
  styleUrls: ['./add-purchased-items-data.component.css']
})
export class AddPurchasedItemsDataComponent {

  items = [
    { name: 'Apple', unit: 'Kg' },
    { name: 'Milk', unit: 'Liter' },
    { name: 'Rice', unit: 'Kg' },
    { name: 'Eggs', unit: 'Dozen' }
  ];

  // Initialize form
  purchaseForm: FormGroup = new FormGroup({
    billNo: new FormControl('', Validators.required),
    billDate: new FormControl('', Validators.required),
    purchaseItems: new FormArray([this.createItem()])
  });

  // Getter for easy access
  get purchaseItems(): FormArray {
    return this.purchaseForm.get('purchaseItems') as FormArray;
  }

  constructor(private toast: ToastService) {}

  // Create a new purchase item row
  createItem(): FormGroup {
    return new FormGroup({
      item: new FormControl('', Validators.required),
      unit: new FormControl({ value: '', disabled: true }),
      quantity: new FormControl(1, [Validators.required, Validators.min(1)]),
      price: new FormControl(0, [Validators.required, Validators.min(0)])
    });
  }

  addItem() {
    this.purchaseItems.push(this.createItem());
  }

  removeItem(index: number) {
    if (this.purchaseItems.length > 1) {
      this.purchaseItems.removeAt(index);
    }
  }

  onItemChange(index: number) {
    const selectedItem = this.items.find(i => i.name === this.purchaseItems.at(index).get('item')?.value);
    if (selectedItem) {
      this.purchaseItems.at(index).get('unit')?.setValue(selectedItem.unit);
    } else {
      this.purchaseItems.at(index).get('unit')?.setValue('');
    }
  }

  submitPurchase() {
    if (this.purchaseForm.invalid) {
      this.toast.warn('Please fill all required fields.');
      return;
    }

    const purchaseData = this.purchaseForm.getRawValue();
    console.log('Purchase Data:', purchaseData);
    this.toast.success('Purchase submitted!');

    // Reset form
    this.purchaseForm.reset();
    this.purchaseItems.clear();
    this.addItem();
  }
}
