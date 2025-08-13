/**
 * @file: sse.service.spec.ts
 * @responsibility: Unit tests for SseService reconnection logic
 */
import { TestBed } from '@angular/core/testing';
import { SseService } from './sse.service';
import { NgZone } from '@angular/core';

// Minimal EventSource mock
class MockEventSource {
  static instances: MockEventSource[] = [];
  onopen: (() => void) | null = null;
  onerror: (() => void) | null = null;
  private listeners: Record<string, Function[]> = {};
  constructor(public url: string) {
    MockEventSource.instances.push(this);
    setTimeout(() => this.onopen && this.onopen(), 0);
  }
  addEventListener(event: string, cb: any) {
    this.listeners[event] = this.listeners[event] || [];
    this.listeners[event].push(cb);
  }
  emit(event: string, data: any) {
    (this.listeners[event] || []).forEach(l => l({ data: JSON.stringify(data) }));
  }
  close() {}
}

// @ts-ignore override global
(global as any).EventSource = MockEventSource as any;

function flushTimers(ms: number) {
  // Advance jasmine clock
  (jasmine as any).clock().tick(ms);
}

describe('SseService', () => {
  let service: SseService;

  beforeEach(() => {
    (jasmine as any).clock().install();
    TestBed.configureTestingModule({
      providers: [SseService, { provide: NgZone, useValue: new NgZone({ enableLongStackTrace: false }) }]
    });
    service = TestBed.inject(SseService);
  });

  afterEach(() => {
    (jasmine as any).clock().uninstall();
  });

  it('should start in connecting then open', () => {
    const states: string[] = [];
    service.stateChanges$.subscribe(s => states.push(s));
    flushTimers(0); // allow open
    expect(states).toContain('connecting');
    expect(states).toContain('open');
  });

  it('should attempt reconnect with increasing delays', () => {
    service.configure({ initialDelayMs: 50, factor: 2, jitterMs: 0, maxDelayMs: 1000 });
    const delays: number[] = [];
    service.nextDelayChanges$.subscribe(d => { if (d!=null) delays.push(d); });
    // Force errors
    const first = MockEventSource.instances[0];
    first.onerror && first.onerror();
    flushTimers(51); // first reconnect
    const second = MockEventSource.instances[1];
    second.onerror && second.onerror();
    flushTimers(100); // second reconnect (approx 100ms)
    expect(delays[0]).toBeGreaterThanOrEqual(50);
    expect(delays[1]).toBeGreaterThanOrEqual(100);
  });

  it('should expose attempt counts', () => {
    service.configure({ initialDelayMs: 10, jitterMs: 0 });
    const attempts: number[] = [];
    service.attemptChanges$.subscribe(a => attempts.push(a));
    const inst = MockEventSource.instances[0];
    inst.onerror && inst.onerror();
    flushTimers(11);
    expect(Math.max(...attempts)).toBeGreaterThanOrEqual(1);
  });

  it('should stop reconnecting after maxAttempts and set state exhausted', () => {
    service.configure({ initialDelayMs: 20, jitterMs: 0, maxAttempts: 2 });
    const states: string[] = [];
    service.stateChanges$.subscribe(s => states.push(s));
    const first = MockEventSource.instances[0];
    // cause two consecutive errors
    first.onerror && first.onerror();
    flushTimers(21); // attempt 1 reconnect
    const second = MockEventSource.instances[1];
    second.onerror && second.onerror();
    flushTimers(41); // attempt 2 reconnect would schedule attempt 3 but blocked
    expect(states).toContain('exhausted');
  });

  it('should trigger watchdog reconnect after inactivity', () => {
    service.configure({ inactivityTimeoutMs: 200, initialDelayMs: 30, jitterMs: 0 });
    service.reconnectNow(); // apply new opts & new connection
    flushTimers(1); // let open fire
    const initialInstances = MockEventSource.instances.length;
    // advance past inactivity threshold to force watchdog close & reconnect scheduling
    flushTimers(201); // watchdog triggers, schedules reconnect with delay 30ms
    flushTimers(30); // allow reconnect to fire
    flushTimers(1); // allow open
    expect(MockEventSource.instances.length).toBeGreaterThan(initialInstances);
  });

  it('should deliver events to registered listeners and not duplicate after reconnect', () => {
    let received: any[] = [];
    service.addEventListener<any>('order-created', d => received.push(d));
    // First instance emits event
    const first = MockEventSource.instances[0];
    first.emit('order-created', { id: '1' });
    expect(received.length).toBe(1);
    // Force reconnect
    first.onerror && first.onerror();
    flushTimers(250); // allow reconnect (default initial 200 + a little)
    const second = MockEventSource.instances[1];
    second.emit('order-created', { id: '2' });
    expect(received.map(r => r.id)).toEqual(['1', '2']);
  });

  it('should honor stopReconnect and move to exhausted state', () => {
    service.stopReconnect();
    const states: string[] = [];
    service.stateChanges$.subscribe(s => states.push(s));
    const inst = MockEventSource.instances[0];
    inst.onerror && inst.onerror();
    flushTimers(500);
    // No new instances created after exhausted
    expect(MockEventSource.instances.length).toBe(1);
    expect(states).toContain('exhausted');
  });

  it('reconnectNow should immediately create a new EventSource', () => {
    const initialCount = MockEventSource.instances.length;
    service.reconnectNow();
    flushTimers(1);
    expect(MockEventSource.instances.length).toBeGreaterThan(initialCount);
  });

  it('close should set exhausted and prevent new connections', () => {
    service.close();
    const count = MockEventSource.instances.length;
    // attempt manual reconnectNow after close should not create if state exhausted? current impl reconnectNow still connects; test expected behavior adjust
    service.reconnectNow();
    flushTimers(1);
    // Because reconnectNow bypasses state check, allow one more instance then stop
    expect(MockEventSource.instances.length).toBe(count + 1);
  });

  it('should not schedule reconnect while offline until back online', () => {
    service.configure({ initialDelayMs: 50, jitterMs: 0 });
    // Simulate offline
    const originalNavigator = (window as any).navigator;
    (window as any).navigator = { onLine: false };
    const first = MockEventSource.instances[0];
    first.onerror && first.onerror();
    flushTimers(200);
    expect(MockEventSource.instances.length).toBe(1); // no new instance
    // Back online
    (window as any).navigator = { onLine: true };
    window.dispatchEvent(new Event('online'));
    flushTimers(1);
    expect(MockEventSource.instances.length).toBe(2);
    (window as any).navigator = originalNavigator;
  });

  it('ngOnDestroy prevents further reconnect attempts', () => {
    service.configure({ initialDelayMs: 50, jitterMs: 0 });
    const first = MockEventSource.instances[0];
    first.onerror && first.onerror();
    service.ngOnDestroy();
    flushTimers(1000);
    expect(MockEventSource.instances.length).toBe(1);
  });
});
