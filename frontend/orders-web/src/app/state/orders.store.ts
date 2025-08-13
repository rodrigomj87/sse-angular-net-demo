/**
 * @file: orders.store.ts
 * @responsibility: Reactive in-memory store for orders list
 */
import { Injectable, effect, inject } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Order } from '../models/order.model';
import { OrdersApiService } from '../services/orders-api.service';
import { SseService } from '../services/sse.service';

interface OrderCreatedEvent { order: Order; }
interface OrderStatusChangedEvent { orderId: string; newStatus: Order['status']; }

@Injectable({ providedIn: 'root' })
export class OrdersStore {
  private readonly orders$ = new BehaviorSubject<Order[]>([]);
  readonly ordersChanges$ = this.orders$.asObservable();

  private api = inject(OrdersApiService);
  private sse = inject(SseService);

  constructor() {
    this.load();
    this.sse.addEventListener<OrderCreatedEvent>('order-created', e => {
      const list = this.orders$.value;
      this.orders$.next([e.order, ...list]);
    });
    this.sse.addEventListener<OrderStatusChangedEvent>('order-status-changed', e => {
      const list = this.orders$.value.map(o => (o.id === e.orderId ? { ...o, status: e.newStatus } : o));
      this.orders$.next(list);
    });
  }

  private load() {
    this.api.list().subscribe(orders => this.orders$.next(orders));
  }
}
