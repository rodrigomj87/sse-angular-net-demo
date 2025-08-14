/**
 * @file: orders.store.ts
 * @responsibility: Reactive in-memory store for orders list
 */
import { Injectable, effect, inject } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Order, OrderStatus } from '../models/order.model';
import { OrdersApiService } from '../services/orders-api.service';
import { SseService } from '../services/sse.service';

// Backend emits order-created with a flat payload (id, status, customerName, totalAmount, createdAt, updatedAt,...)
interface OrderCreatedEvent {
  id: string;
  status: string;
  customerName: string;
  totalAmount: number;
  createdAt: string;
  updatedAt: string;
  code?: string;
  traceId?: string;
}
interface OrderStatusChangedEvent { id: string; newStatus: string; previousStatus?: string; updatedAt?: string; }

@Injectable({ providedIn: 'root' })
export class OrdersStore {
  private readonly orders$ = new BehaviorSubject<Order[]>([]);
  readonly ordersChanges$ = this.orders$.asObservable();

  private api = inject(OrdersApiService);
  private sse = inject(SseService);

  constructor() {
    this.load();
    this.sse.addEventListener<OrderCreatedEvent>('order-created', e => {
      const normalized = this.normalizeCreated(e as any);
      // de-dup in case order already present
      const existing = this.orders$.value.filter(o => o.id !== normalized.id);
      this.orders$.next([normalized, ...existing]);
    });
    this.sse.addEventListener<OrderStatusChangedEvent>('order-status-changed', e => {
      const list = this.orders$.value.map(o => (
        o.id === e.id
          ? { ...o, status: (e.newStatus as string).toUpperCase() as OrderStatus, updatedAt: e.updatedAt ?? o.updatedAt }
          : o
      ));
      this.orders$.next(list);
    });
  }

  private load() {
    this.api.list().subscribe(orders => this.orders$.next(orders.map(o => this.normalizeCreated(o))));
  }

  private normalizeCreated(o: any): Order {
    return {
      id: o.id,
      customerName: o.customerName,
      totalAmount: o.totalAmount,
      status: (o.status as string).toUpperCase() as OrderStatus,
      createdAt: o.createdAt,
      updatedAt: o.updatedAt
    };
  }
}
