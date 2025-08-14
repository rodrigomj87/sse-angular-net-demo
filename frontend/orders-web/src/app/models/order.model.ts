/**
 * @file: order.model.ts
 * @responsibility: Order domain interfaces/types for frontend use
 */
export type OrderStatus = 'CREATED' | 'PAID' | 'FULFILLED' | 'CANCELLED';

export interface Order {
  id: string;
  customerName: string;
  totalAmount: number;
  status: OrderStatus;
  createdAt: string;
  updatedAt: string;
}

export interface CreateOrderDto {
  customerName: string;
  totalAmount: number;
}

export interface SseEvent<T = unknown> {
  id?: string;
  event: string;
  data: T;
}
