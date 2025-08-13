/**
 * @file: orders-api.service.ts
 * @responsibility: HTTP operations for Orders
 */
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CreateOrderDto, Order } from '../models/order.model';

@Injectable({ providedIn: 'root' })
export class OrdersApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/orders`;

  list(): Observable<Order[]> {
    return this.http.get<Order[]>(this.base);
  }

  create(dto: CreateOrderDto): Observable<Order> {
    return this.http.post<Order>(this.base, dto);
  }
}
