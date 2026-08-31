import { ApplicationConfig, importProvidersFrom, APP_INITIALIZER } from '@angular/core';
import { provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideAnimations, provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { providePrimeNG } from 'primeng/config';
import { apiInterceptor } from './interceptors/api.interceptor';
import { RuntimeConfigService, loadRuntimeConfig } from './Services/runtime-config.service';
import { MessageService } from 'primeng/api';
import { definePreset } from '@primeuix/themes';
import Aura from '@primeuix/themes/aura';
import { routes } from './app.routes';

// "Forest Green + Amber" preset — keeps Aura's structure/behavior, swaps the
// brand colors so every PrimeNG component (buttons, inputs, tables, dialogs)
// matches the DFC identity without per-component overrides.
const DfcPreset = definePreset(Aura, {
  semantic: {
    primary: {
      50: '{amber.50}',
      100: '{amber.100}',
      200: '{amber.200}',
      300: '{amber.300}',
      400: '{amber.400}',
      500: '{amber.500}',
      600: '{amber.600}',
      700: '{amber.700}',
      800: '{amber.800}',
      900: '{amber.900}',
      950: '{amber.950}'
    },
    colorScheme: {
      light: {
        primary: {
          color: '{amber.500}',
          contrastColor: '#081f13',
          hoverColor: '{amber.600}',
          activeColor: '{amber.700}'
        },
        surface: {
          0: '#ffffff',
          50: '{slate.50}',
          100: '{slate.100}',
          200: '{slate.200}',
          300: '{slate.300}',
          400: '{slate.400}',
          500: '{slate.500}',
          600: '{slate.600}',
          700: '{slate.700}',
          800: '{slate.800}',
          900: '{slate.900}',
          950: '{slate.950}'
        }
      }
    }
  }
});

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    // Phase 9: runtime config loaded before the app starts.
    {
      provide: APP_INITIALIZER,
      useFactory: loadRuntimeConfig,
      deps: [RuntimeConfigService],
      multi: true,
    },
    // Phase 9 (+ Angular half of 6 & 8): endpoint failover + bearer + Idempotency-Key + 401 handling.
    provideHttpClient(withInterceptors([apiInterceptor])),
    providePrimeNG({
      theme: {
        preset: DfcPreset,
        options: { darkModeSelector: false }
      },
      ripple: true
    }),
    MessageService // ✅ provide it here for global DI
  ]
};
