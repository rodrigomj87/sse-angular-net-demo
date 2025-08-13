/**
 * @file: connection-status.component.ts
 * @responsibility: Display SSE connection status
 */
import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SseService } from '../../services/sse.service';

@Component({
  selector: 'app-connection-status',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span class="status"
      [class.open]="state() === 'open'"
      [class.closed]="state() === 'closed'"
      [class.connecting]="state() === 'connecting'"
      [class.exhausted]="state() === 'exhausted'"
    >
      {{ state() }}
    </span>
  `,
  styles: [
  `.status { padding:2px 8px; border-radius:12px; font-size:12px; text-transform:uppercase; letter-spacing:.5px; background:#ccc; color:#222; }
   .status.open { background:#2e7d32; color:#fff; }
   .status.closed { background:#c62828; color:#fff; }
   .status.connecting { background:#f9a825; color:#222; }
   .status.exhausted { background:#6d4c41; color:#fff; }`
  ]
})
export class ConnectionStatusComponent {
  private sse = inject(SseService);
  state = signal<'connecting' | 'open' | 'closed' | 'exhausted'>('connecting');

  constructor() {
    this.sse.stateChanges$.subscribe(s => this.state.set(s));
  }
}
