/**
 * @file: order-create-form.component.ts
 * @responsibility: Reactive form to create a new order
 */
import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { OrdersApiService } from '../../services/orders-api.service';
import { OrdersStore } from '../../state/orders.store';

@Component({
  selector: 'app-order-create-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  template: `
    <form [formGroup]="form" (ngSubmit)="submit()" class="order-form">
      <input formControlName="description" placeholder="Description" />
      <button type="submit" [disabled]="form.invalid || submitting()">Create</button>
    </form>
  `,
  styles: [`.order-form { display:flex; gap:8px; margin-bottom:1rem; }
            .order-form input { flex:1; padding:4px 8px; }
            .order-form button { padding:4px 12px; }
            `]
})
export class OrderCreateFormComponent {
  private fb = inject(FormBuilder);
  private api = inject(OrdersApiService);
  private store = inject(OrdersStore);
  submitting = signal(false);

  form = this.fb.group({
    description: ['', [Validators.required, Validators.minLength(3)]]
  });

  submit() {
    if (this.form.invalid) return;
    this.submitting.set(true);
    this.api.create({ description: this.form.value.description! }).subscribe({
      next: () => {
        this.form.reset();
        this.submitting.set(false);
      },
      error: () => this.submitting.set(false)
    });
  }
}
