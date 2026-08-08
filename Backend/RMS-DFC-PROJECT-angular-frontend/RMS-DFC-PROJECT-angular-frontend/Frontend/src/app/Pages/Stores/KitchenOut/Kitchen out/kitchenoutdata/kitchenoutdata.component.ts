import { Component } from '@angular/core';
import { FormGroup, FormArray, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';

import { RouterOutlet } from '@angular/router';
import { ToastService } from '../../../../../Services/toast.service';

@Component({
  selector: 'app-kitchenoutdata',
  standalone: true,
  imports: [ReactiveFormsModule, RouterOutlet],
  templateUrl: './kitchenoutdata.component.html',
  styleUrls: ['./kitchenoutdata.component.css']
})
export class KitchenOutDataComponent {

  items = [
    { name: 'Apple', unit: 'Kg' },
    { name: 'Milk', unit: 'Liter' },
    { name: 'Rice', unit: 'Kg' },
    { name: 'Eggs', unit: 'Dozen' }
  ];

  // Initialize form
  kitchenOutForm: FormGroup = new FormGroup({
    outNo: new FormControl('', Validators.required),
    outDate: new FormControl('', Validators.required),
    kitchenItems: new FormArray([this.createItem()])
  });

  // Getter for FormArray
  get kitchenItems(): FormArray {
    return this.kitchenOutForm.get('kitchenItems') as FormArray;
  }

  constructor(private toast: ToastService) {}

  // Create a new row
  createItem(): FormGroup {
    return new FormGroup({
      item: new FormControl('', Validators.required),
      unit: new FormControl({ value: '', disabled: true }),
      quantity: new FormControl(1, [Validators.required, Validators.min(1)])
    });
  }

  // Add new row
  addItem() {
    this.kitchenItems.push(this.createItem());
  }

  // Remove row
  removeItem(index: number) {
    if (this.kitchenItems.length > 1) {
      this.kitchenItems.removeAt(index);
    }
  }

  // Update unit when item is selected
  onItemChange(index: number) {
    const selectedItem = this.items.find(i => i.name === this.kitchenItems.at(index).get('item')?.value);
    if (selectedItem) {
      this.kitchenItems.at(index).get('unit')?.setValue(selectedItem.unit);
    } else {
      this.kitchenItems.at(index).get('unit')?.setValue('');
    }
  }

  // Submit form
  submitKitchenOut() {
    if (this.kitchenOutForm.invalid) {
      this.toast.warn('Please fill all required fields.');
      return;
    }

    const kitchenData = this.kitchenOutForm.getRawValue();
    console.log('Kitchen Out Data:', kitchenData);
    this.toast.success('Data submitted!');

    // Reset form
    this.kitchenOutForm.reset();
    this.kitchenItems.clear();
    this.addItem();
  }
}
