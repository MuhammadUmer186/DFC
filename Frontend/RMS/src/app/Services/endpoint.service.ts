import { Injectable, signal } from '@angular/core';
import { RuntimeConfigService } from './runtime-config.service';

export type OperatingMode = 'LOCAL' | 'CLOUD' | 'OFFLINE';

/**
 * Phase 9 — endpoint selection & failover.
 *  - probe the edge first (short timeout); prefer LOCAL
 *  - after N consecutive edge failures with cloud reachable → CLOUD
 *  - neither reachable → OFFLINE
 *  - never switch while a critical transaction (order/payment) is in flight
 *  - keep probing the edge in the background; switch back only when idle
 */
@Injectable({ providedIn: 'root' })
export class EndpointService {
  readonly mode = signal<OperatingMode>('LOCAL');
  readonly edgeHealthy = signal<boolean>(true);
  readonly cloudHealthy = signal<boolean>(true);
  readonly lastProbeUtc = signal<string | null>(null);

  private edgeFailures = 0;
  private criticalDepth = 0;
  private started = false;

  constructor(private rc: RuntimeConfigService) {}

  /** Base API URL for the current mode. */
  apiBase(): string {
    const c = this.rc.config;
    return this.mode() === 'CLOUD' ? c.cloudApiUrl : c.edgeApiUrl;
  }

  /** SignalR hub URL for the current mode. */
  hubUrl(): string {
    const c = this.rc.config;
    return this.mode() === 'CLOUD' ? c.cloudHubUrl : c.edgeHubUrl;
  }

  /** Wrap an order/payment flow so failover can't swap servers mid-transaction. */
  beginCritical(): void {
    this.criticalDepth++;
  }
  endCritical(): void {
    this.criticalDepth = Math.max(0, this.criticalDepth - 1);
  }
  get inCriticalTransaction(): boolean {
    return this.criticalDepth > 0;
  }

  start(): void {
    if (this.started) return;
    this.started = true;
    void this.probe();
    setInterval(() => void this.probe(), this.rc.config.failover.recoveryProbeIntervalMs);
  }

  private async probe(): Promise<void> {
    const c = this.rc.config;
    const edgeOk = await this.ping(c.edgeApiUrl);
    this.edgeHealthy.set(edgeOk);
    this.lastProbeUtc.set(new Date().toISOString());

    if (edgeOk) {
      this.edgeFailures = 0;
      if (this.mode() !== 'LOCAL' && !this.inCriticalTransaction) this.mode.set('LOCAL');
      this.cloudHealthy.set(true);
      return;
    }

    this.edgeFailures++;
    if (this.edgeFailures < c.failover.consecutiveFailuresToCloud) return;

    const cloudOk = await this.ping(c.cloudApiUrl);
    this.cloudHealthy.set(cloudOk);
    if (cloudOk && !this.inCriticalTransaction) this.mode.set('CLOUD');
    else if (!cloudOk) this.mode.set('OFFLINE');
  }

  private async ping(apiBase: string): Promise<boolean> {
    const url = apiBase.replace(/\/api\/?$/, '') + '/health/ready';
    const ctrl = new AbortController();
    const t = setTimeout(() => ctrl.abort(), this.rc.config.failover.edgeProbeTimeoutMs);
    try {
      const r = await fetch(url, { signal: ctrl.signal, cache: 'no-store' });
      return r.ok;
    } catch {
      return false;
    } finally {
      clearTimeout(t);
    }
  }
}
