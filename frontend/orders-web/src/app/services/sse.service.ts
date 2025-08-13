/**
 * @file: sse.service.ts
 * @responsibility: Manage SSE connection with auto-reconnect and typed listeners
 */
import { Injectable, NgZone } from '@angular/core';
import { BehaviorSubject, Subject, timer } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { SseEvent } from '../models/order.model';

interface Listener<T = any> {
  event: string;
  cb: (payload: T) => void;
}

@Injectable({ providedIn: 'root' })
export class SseService {
  private source?: EventSource;
  private reconnectAttempts = 0;
  private readonly maxDelayMs = 30000;
  private readonly destroyed$ = new Subject<void>();
  private readonly connectionState$ = new BehaviorSubject<'connecting' | 'open' | 'closed'>('connecting');
  private listeners: Listener[] = [];

  stateChanges$ = this.connectionState$.asObservable();

  constructor(private zone: NgZone) {
    this.connect();
  }

  addEventListener<T>(event: string, cb: (payload: T) => void) {
    this.listeners.push({ event, cb });
  }

  private connect() {
    this.connectionState$.next('connecting');
    const url = environment.sseUrl;
    this.zone.runOutsideAngular(() => {
      this.source = new EventSource(url, { withCredentials: false });

      this.source.onopen = () => {
        this.zone.run(() => {
          this.reconnectAttempts = 0;
          this.connectionState$.next('open');
        });
      };

      this.source.onerror = () => {
        this.zone.run(() => {
          this.connectionState$.next('closed');
          this.scheduleReconnect();
        });
      };

      this.listeners.forEach(l => {
        this.source!.addEventListener(l.event, (e: MessageEvent) => {
          try {
            const parsed: SseEvent = { event: l.event, data: JSON.parse(e.data) };
            this.zone.run(() => l.cb(parsed.data));
          } catch {
            // swallow parse errors
          }
        });
      });
    });
  }

  private scheduleReconnect() {
    if (this.source) {
      this.source.close();
      this.source = undefined;
    }
    this.reconnectAttempts++;
    const delay = Math.min(this.backoff(this.reconnectAttempts), this.maxDelayMs);
    timer(delay).pipe(takeUntil(this.destroyed$)).subscribe(() => this.connect());
  }

  private backoff(attempt: number): number {
    // Exponential backoff with jitter
    const base = Math.pow(2, attempt) * 100; // start at 100ms
    const jitter = Math.random() * 100;
    return base + jitter;
  }

  ngOnDestroy() {
    this.destroyed$.next();
    this.destroyed$.complete();
    if (this.source) {
      this.source.close();
    }
  }
}
