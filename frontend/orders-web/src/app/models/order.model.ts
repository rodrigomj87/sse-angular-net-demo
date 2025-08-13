/**
 * @file: order.model.ts
 * @responsibility: Order domain interfaces/types for frontend use
 */
export type OrderStatus = 'Created' | 'Processing' | 'Completed' | 'Cancelled';

export interface Order {
  id: string;
  description: string;
  status: OrderStatus;
  createdAt: string; // ISO string
}

export interface CreateOrderDto {
  description: string;
}

export interface SseEvent<T = unknown> {
  id?: string;
  event: string;
  data: T;
}
