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

// Override browser EventSource for tests
(window as any).EventSource = MockEventSource as any;

function flushTimers(ms: number) {
  // Advance jasmine clock
  (jasmine as any).clock().tick(ms);
}

describe('SseService', () => {
  let service: SseService;
  let onlineStatus = true;

  beforeEach(() => {
    (jasmine as any).clock().install();
    (MockEventSource as any).instances = [];
    onlineStatus = true;
    TestBed.configureTestingModule({
      providers: [SseService, { provide: NgZone, useValue: new NgZone({ enableLongStackTrace: false }) }]
    });
    spyOnProperty(window.navigator, 'onLine', 'get').and.callFake(() => onlineStatus);
    service = TestBed.inject(SseService);
  });

  afterEach(() => {
    try { service.ngOnDestroy(); } catch {}
    (jasmine as any).clock().uninstall();
  });

  it('should start in connecting then open', () => {
    const states: string[] = [];
    service.stateChanges$.subscribe(s => states.push(s));
    expect(states[0]).toBe('connecting');
    flushTimers(0); // allow queued onopen
    expect(states).toContain('open');
  });

  it('should attempt reconnect with non-decreasing delays', () => {
    service.configure({ initialDelayMs: 50, factor: 2, jitterMs: 0, maxDelayMs: 1000 });
    const delays: number[] = [];
    service.nextDelayChanges$.subscribe(d => { if (d!=null) delays.push(d); });
    const first = MockEventSource.instances[0];
    first.onerror && first.onerror();
    flushTimers(51);
    const second = MockEventSource.instances[1];
    second.onerror && second.onerror();
    flushTimers(101);
    expect(delays.length).toBeGreaterThanOrEqual(2);
    expect(delays[0]!).toBeGreaterThanOrEqual(50);
    expect(delays[1]!).toBeGreaterThanOrEqual(delays[0]!); // monotonic
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

  it('should reset attempts after open (no exhaustion)', () => {
    service.configure({ initialDelayMs: 20, jitterMs: 0, maxAttempts: 2 });
    const first = MockEventSource.instances[0];
    first.onerror && first.onerror();
    flushTimers(21); // reconnect
    flushTimers(0); // open second
    const second = MockEventSource.instances[1];
    second.onerror && second.onerror();
    flushTimers(21); // reconnect third
    // service should still operate (not exhausted because attempts reset on open)
    expect(MockEventSource.instances.length).toBeGreaterThanOrEqual(3);
  });

  it('should trigger watchdog reconnect after inactivity', () => {
    service.configure({ inactivityTimeoutMs: 200, initialDelayMs: 30, jitterMs: 0 });
  service.reconnectNow();
  flushTimers(0); // open with new options
  const initial = MockEventSource.instances.length;
    (service as any).lastEventTime = Date.now() - 1000; // stale
  flushTimers(201); // reach threshold (schedule reconnect after 30ms)
    flushTimers(5); // allow subscription callback execution
  flushTimers(31); // allow reconnect timer (30ms +1)
  flushTimers(1); // process onopen (setTimeout 0)
    expect(MockEventSource.instances.length).toBeGreaterThan(initial);
  });

  it('should deliver events to registered listeners and not duplicate after reconnect', () => {
    const received: any[] = [];
    service.addEventListener<any>('order-created', d => received.push(d));
    service.reconnectNow();
    flushTimers(0); // open with listener bound
    const first = MockEventSource.instances[MockEventSource.instances.length - 1];
    first.emit('order-created', { id: '1' });
    flushTimers(0);
    expect(received.length).toBe(1);
    first.onerror && first.onerror();
    flushTimers(250);
    flushTimers(0);
    const second = MockEventSource.instances[MockEventSource.instances.length - 1];
    second.emit('order-created', { id: '2' });
    expect(received.map(r => r.id)).toEqual(['1', '2']);
  });

  it('should honor stopReconnect and avoid new instances', () => {
    flushTimers(0); // open
    const initial = MockEventSource.instances.length;
    service.stopReconnect();
    const inst = MockEventSource.instances[0];
    inst.onerror && inst.onerror();
    flushTimers(500);
    // stopReconnect sets state to exhausted but existing instance may remain; ensure no new
    expect(MockEventSource.instances.length).toBe(initial);
  });

  it('reconnectNow should immediately create a new EventSource', () => {
    const initialCount = MockEventSource.instances.length;
    service.reconnectNow();
    flushTimers(1);
    expect(MockEventSource.instances.length).toBeGreaterThan(initialCount);
  });

  it('close should prevent new connections even via reconnectNow', () => {
    flushTimers(0); // open
    service.close();
    const count = MockEventSource.instances.length;
    service.reconnectNow();
    flushTimers(1);
    expect(MockEventSource.instances.length).toBe(count); // no new
  });

  it('should not schedule reconnect while offline until back online', () => {
    service.configure({ initialDelayMs: 50, jitterMs: 0 });
    flushTimers(0); // open
    onlineStatus = false;
    const first = MockEventSource.instances[0];
    first.onerror && first.onerror();
    flushTimers(200);
    expect(MockEventSource.instances.length).toBe(1);
    onlineStatus = true;
    window.dispatchEvent(new Event('online'));
    flushTimers(1); // reconnectNow
    flushTimers(0); // open
    expect(MockEventSource.instances.length).toBe(2);
  });

  it('ngOnDestroy prevents further reconnect attempts', () => {
    service.configure({ initialDelayMs: 50, jitterMs: 0 });
    flushTimers(0); // open
    const first = MockEventSource.instances[0];
    first.onerror && first.onerror();
    service.ngOnDestroy();
    flushTimers(1000);
    expect(MockEventSource.instances.length).toBe(1);
  });
});
