import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';

/**
 * Phase 9 — runtime (not compile-time) configuration. Fetched once at bootstrap
 * from `/runtime-config.json` (served by nginx, mounted per-deployment) so the
 * same build can point at the edge or the cloud.
 */
export interface RuntimeConfig {
  edgeApiUrl: string;
  cloudApiUrl: string;
  edgeHubUrl: string;
  cloudHubUrl: string;
  failover: {
    edgeProbeTimeoutMs: number;
    consecutiveFailuresToCloud: number;
    recoveryProbeIntervalMs: number;
    statusPollMs: number;
  };
}

const FALLBACK: RuntimeConfig = {
  edgeApiUrl: environment.apiBaseUrl,
  cloudApiUrl: environment.apiBaseUrl,
  edgeHubUrl: (environment.apihub || '') + '/hubs/orders',
  cloudHubUrl: (environment.apihub || '') + '/hubs/orders',
  failover: {
    edgeProbeTimeoutMs: 1500,
    consecutiveFailuresToCloud: 3,
    recoveryProbeIntervalMs: 15000,
    statusPollMs: 20000,
  },
};

@Injectable({ providedIn: 'root' })
export class RuntimeConfigService {
  private cfg: RuntimeConfig = FALLBACK;

  get config(): RuntimeConfig {
    return this.cfg;
  }

  /** APP_INITIALIZER hook. Never rejects — falls back to compile-time env. */
  async load(): Promise<void> {
    try {
      const res = await fetch('runtime-config.json', { cache: 'no-store' });
      if (res.ok) {
        const json = (await res.json()) as Partial<RuntimeConfig>;
        this.cfg = {
          ...FALLBACK,
          ...json,
          failover: { ...FALLBACK.failover, ...(json.failover ?? {}) },
        };
      }
    } catch {
      /* keep FALLBACK */
    }
  }
}

export function loadRuntimeConfig(svc: RuntimeConfigService) {
  return () => svc.load();
}
