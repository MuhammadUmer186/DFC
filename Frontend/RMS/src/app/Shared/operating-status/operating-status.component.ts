import { Component, OnDestroy, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { EndpointService } from '../../Services/endpoint.service';

interface NodeStatus {
  nodeRole?: string;
  operatingMode?: string;
  lastSuccessfulPushUtc?: string | null;
  lastSuccessfulPullUtc?: string | null;
  pendingOutboxCount?: number;
  deadLetterCount?: number;
  conflictCount?: number;
  lastPeerHeartbeatUtc?: string | null;
  databaseConnected?: boolean;
}

/**
 * Phase 9/17 — the operating-status widget for the RMS header/sidebar.
 * Accessible colours + tooltips; additive, does not alter existing layout.
 */
@Component({
  selector: 'app-operating-status',
  standalone: true,
  imports: [CommonModule],
  styles: [
    `
      .ops { display: inline-flex; align-items: center; gap: .5rem; font-size: .8rem; }
      .dot { width: .6rem; height: .6rem; border-radius: 50%; display: inline-block; }
      .LOCAL { background: #1b7f3b; }   /* green  */
      .CLOUD { background: #b26a00; }   /* amber  */
      .OFFLINE { background: #b00020; } /* red    */
      .pill { padding: .1rem .45rem; border-radius: 999px; background: rgba(0,0,0,.06); }
      .warn { color: #b00020; font-weight: 600; }
    `,
  ],
  template: `
    <span
      class="ops"
      [attr.title]="tooltip()"
      role="status"
      aria-live="polite"
    >
      <span class="dot" [class]="mode()"></span>
      <span class="pill">{{ mode() }}</span>
      <span *ngIf="s() as st">
        <ng-container *ngIf="st.pendingOutboxCount">· {{ st.pendingOutboxCount }} pending</ng-container>
        <ng-container *ngIf="st.conflictCount" class="warn"> · {{ st.conflictCount }} conflicts</ng-container>
        <ng-container *ngIf="st.deadLetterCount" class="warn"> · {{ st.deadLetterCount }} dead</ng-container>
      </span>
      <span *ngIf="lastSync()">· synced {{ lastSync() }}</span>
    </span>
  `,
})
export class OperatingStatusComponent implements OnInit, OnDestroy {
  private http = inject(HttpClient);
  private endpoints = inject(EndpointService);

  readonly mode = this.endpoints.mode;
  readonly s = signal<NodeStatus | null>(null);
  private timer: any;

  readonly lastSync = computed(() => {
    const st = this.s();
    const t = st?.lastSuccessfulPushUtc || st?.lastSuccessfulPullUtc;
    if (!t) return '';
    const secs = Math.max(0, Math.round((Date.now() - new Date(t).getTime()) / 1000));
    return secs < 90 ? `${secs}s ago` : `${Math.round(secs / 60)}m ago`;
  });

  readonly tooltip = computed(() => {
    const st = this.s();
    return [
      `Operating mode: ${this.mode()}`,
      `Edge healthy: ${this.endpoints.edgeHealthy()}`,
      `Cloud healthy: ${this.endpoints.cloudHealthy()}`,
      st ? `DB connected: ${st.databaseConnected}` : '',
      st ? `Pending outbox: ${st.pendingOutboxCount ?? 0}` : '',
      st ? `Conflicts: ${st.conflictCount ?? 0} · Dead letters: ${st.deadLetterCount ?? 0}` : '',
      st?.lastPeerHeartbeatUtc ? `Last peer heartbeat: ${st.lastPeerHeartbeatUtc}` : '',
    ]
      .filter(Boolean)
      .join('\n');
  });

  ngOnInit(): void {
    this.endpoints.start();
    this.poll();
    this.timer = setInterval(() => this.poll(), 20000);
  }
  ngOnDestroy(): void {
    clearInterval(this.timer);
  }

  private poll(): void {
    // `/api` is rewritten to the selected node by the interceptor.
    this.http.get<NodeStatus>(`${environment.apiBaseUrl}/system/node-status`).subscribe({
      next: (r) => this.s.set(r),
      error: () => {},
    });
  }
}
