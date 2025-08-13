/**
 * @file: orders-list.component.ts
 * @responsibility: Render list of orders with real-time updates
 */
import { Component, inject } from '@angular/core';
import { AsyncPipe, DatePipe, NgFor, NgIf } from '@angular/common';
import { OrdersStore } from '../../state/orders.store';

@Component({
  selector: 'app-orders-list',
  standalone: true,
  imports: [NgFor, NgIf, AsyncPipe, DatePipe],
  template: `
    <div *ngIf="orders$ | async as orders; else loading">
      <div *ngIf="orders.length; else empty">
        <table class="orders">
          <thead>
            <tr><th>ID</th><th>Description</th><th>Status</th><th>Created</th></tr>
          </thead>
            <tbody>
              <tr *ngFor="let o of orders">
                <td>{{ o.id }}</td>
                <td>{{ o.description }}</td>
                <td>{{ o.status }}</td>
                <td>{{ o.createdAt | date:'short' }}</td>
              </tr>
            </tbody>
        </table>
      </div>
      <ng-template #empty><p>No orders yet.</p></ng-template>
    </div>
    <ng-template #loading><p>Loading...</p></ng-template>
  `,
  styles: [`table.orders { width:100%; border-collapse:collapse; }
            table.orders th, table.orders td { padding:4px 8px; border-bottom:1px solid #e0e0e0; font-size:13px; }
            table.orders th { text-align:left; background:#fafafa; }
            `]
})
export class OrdersListComponent {
  private store = inject(OrdersStore);
  orders$ = this.store.ordersChanges$;
}
