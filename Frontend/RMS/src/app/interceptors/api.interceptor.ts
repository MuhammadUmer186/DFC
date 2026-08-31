import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, finalize, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import { EndpointService } from '../Services/endpoint.service';

/**
 * Phase 9 (+ Angular half of Phases 6 & 8). One place for:
 *  - rewriting API calls to the currently-selected node (edge/cloud),
 *  - attaching the bearer token,
 *  - attaching an Idempotency-Key to mutating requests,
 *  - standardised 401/403 handling.
 * Existing services keep calling `environment.apiBaseUrl + '/...'` unchanged.
 */
const COMPILE_TIME_BASE = environment.apiBaseUrl.replace(/\/$/, '');

const CRITICAL = [/\/orders(\/|$|\?)/i, /\/orders\/pay/i, /\/confirm-payment/i];
const NO_IDEMPOTENCY = [/\/Auth\/Login/i, /\/api\/sync\//i];

export const apiInterceptor: HttpInterceptorFn = (req, next) => {
  const endpoints = inject(EndpointService);
  const router = inject(Router);

  // --- rewrite origin to the selected node ---------------------------------
  let url = req.url;
  const isApiCall =
    url.startsWith(COMPILE_TIME_BASE) || url.startsWith('/api/') || url.startsWith('/hubs/');
  if (isApiCall) {
    const base = endpoints.apiBase().replace(/\/$/, '');
    if (url.startsWith(COMPILE_TIME_BASE)) url = base + url.slice(COMPILE_TIME_BASE.length);
    else if (url.startsWith('/api')) url = base + url.slice('/api'.length);
  }

  // --- headers -----------------------------------------------------------
  const setHeaders: Record<string, string> = {};
  const token = localStorage.getItem('token');
  if (token && !req.headers.has('Authorization')) setHeaders['Authorization'] = `Bearer ${token}`;

  const mutating = ['POST', 'PUT', 'PATCH', 'DELETE'].includes(req.method.toUpperCase());
  if (mutating && !NO_IDEMPOTENCY.some((r) => r.test(url)) && !req.headers.has('Idempotency-Key')) {
    setHeaders['Idempotency-Key'] = randomUuid();
  }

  const isCritical = mutating && CRITICAL.some((r) => r.test(url));
  if (isCritical) endpoints.beginCritical();

  return next(req.clone({ url, setHeaders })).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401) {
        localStorage.removeItem('token');
        localStorage.removeItem('role');
        router.navigate(['/login']);
      }
      return throwError(() => err);
    }),
    finalize(() => {
      if (isCritical) endpoints.endCritical();
    }),
  );
};

function randomUuid(): string {
  const c = (globalThis as any).crypto;
  if (c?.randomUUID) return c.randomUUID();
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (ch) => {
    const r = (Math.random() * 16) | 0;
    return (ch === 'x' ? r : (r & 0x3) | 0x8).toString(16);
  });
}
