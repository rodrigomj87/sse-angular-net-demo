/**
 * @file: sse.service.ts
 * @responsibility: Manage SSE connection with auto-reconnect and typed listeners
 */
import { Injectable, NgZone } from '@angular/core';
import { BehaviorSubject, Subject, timer, fromEvent, merge, Subscription } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { SseEvent } from '../models/order.model';

interface Listener<T = any> {
  event: string;
  cb: (payload: T) => void;
}

export interface SseBackoffOptions {
  initialDelayMs?: number; // default 200
  factor?: number; // default 2
  jitterMs?: number; // default 100
  maxDelayMs?: number; // default 30000
  maxAttempts?: number; // undefined = infinite
  inactivityTimeoutMs?: number; // watchdog, default 35000
}

type InternalState = 'connecting' | 'open' | 'closed' | 'exhausted';

@Injectable({ providedIn: 'root' })
export class SseService {
  private source?: EventSource;
  private reconnectAttempts = 0;
  private lastEventTime = Date.now();
  private readonly destroyed$ = new Subject<void>();
  private readonly connectionState$ = new BehaviorSubject<InternalState>('connecting');
  private readonly attempt$ = new BehaviorSubject<number>(0);
  private readonly nextDelay$ = new BehaviorSubject<number | null>(null);
  private listeners: Listener[] = [];
  private opts: Required<SseBackoffOptions> = {
    initialDelayMs: 200,
    factor: 2,
    jitterMs: 100,
    maxDelayMs: 30000,
    maxAttempts: undefined as any,
    inactivityTimeoutMs: 35000
  };
  private offlineSub?: Subscription;
  private watchdogTimerSub?: Subscription;

  stateChanges$ = this.connectionState$.asObservable();
  attemptChanges$ = this.attempt$.asObservable();
  nextDelayChanges$ = this.nextDelay$.asObservable();

  constructor(private zone: NgZone) {
    this.bindOnlineOffline();
    this.connect();
  }

  addEventListener<T>(event: string, cb: (payload: T) => void) {
    this.listeners.push({ event, cb });
  }

  configure(options: SseBackoffOptions) {
    this.opts = { ...this.opts, ...options };
  }

  private connect() {
    if (this.connectionState$.value === 'exhausted') return;
    this.connectionState$.next('connecting');
    const url = environment.sseUrl;
    this.zone.runOutsideAngular(() => {
      this.clearWatchdog();
      this.source = new EventSource(url, { withCredentials: false });
      this.setupWatchdog();

      this.source.onopen = () => {
        this.zone.run(() => {
          this.reconnectAttempts = 0;
          this.attempt$.next(0);
          this.nextDelay$.next(null);
          this.lastEventTime = Date.now();
          this.connectionState$.next('open');
        });
      };

      this.source.onerror = () => {
        this.zone.run(() => {
          if (navigator.onLine === false) {
            this.connectionState$.next('closed');
            // wait for online event to reconnect
            return;
          }
          this.connectionState$.next('closed');
          this.scheduleReconnect();
        });
      };

      this.listeners.forEach(l => {
        this.source!.addEventListener(l.event, (e: MessageEvent) => {
          try {
            const parsed: SseEvent = { event: l.event, data: JSON.parse(e.data) };
            this.lastEventTime = Date.now();
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
    if (this.opts.maxAttempts && this.reconnectAttempts >= this.opts.maxAttempts) {
      this.connectionState$.next('exhausted');
      return;
    }
    this.reconnectAttempts++;
    this.attempt$.next(this.reconnectAttempts);
    const delay = Math.min(this.backoff(this.reconnectAttempts), this.opts.maxDelayMs);
    this.nextDelay$.next(delay);
    timer(delay).pipe(takeUntil(this.destroyed$)).subscribe(() => this.connect());
  }

  private backoff(attempt: number): number {
    const base = this.opts.initialDelayMs * Math.pow(this.opts.factor, attempt - 1);
    const jitter = Math.random() * this.opts.jitterMs;
    return base + jitter;
  }

  reconnectNow() {
    if (this.source) {
      this.source.close();
      this.source = undefined;
    }
    this.connect();
  }

  stopReconnect() {
    this.connectionState$.next('exhausted');
  }

  close() {
    this.stopReconnect();
    if (this.source) {
      this.source.close();
      this.source = undefined;
    }
  }

  private bindOnlineOffline() {
    this.zone.runOutsideAngular(() => {
      const online$ = fromEvent(window, 'online').pipe(filter(() => navigator.onLine));
      const offline$ = fromEvent(window, 'offline');
      this.offlineSub = merge(online$, offline$)
        .pipe(takeUntil(this.destroyed$))
        .subscribe(evt => {
          if (evt.type === 'online' && this.connectionState$.value !== 'open') {
            this.reconnectNow();
          }
        });
    });
  }

  private setupWatchdog() {
    this.clearWatchdog();
    this.watchdogTimerSub = timer(this.opts.inactivityTimeoutMs, this.opts.inactivityTimeoutMs)
      .pipe(takeUntil(this.destroyed$))
      .subscribe(() => {
        const delta = Date.now() - this.lastEventTime;
        if (delta >= this.opts.inactivityTimeoutMs && this.connectionState$.value === 'open') {
          // Force reconnect if stale
            this.source?.close();
            this.source = undefined;
            this.connectionState$.next('closed');
            this.scheduleReconnect();
        }
      });
  }

  private clearWatchdog() {
    this.watchdogTimerSub?.unsubscribe();
    this.watchdogTimerSub = undefined;
  }

  ngOnDestroy() {
    this.destroyed$.next();
    this.destroyed$.complete();
    this.clearWatchdog();
    this.offlineSub?.unsubscribe();
    if (this.source) {
      this.source.close();
    }
  }
}
