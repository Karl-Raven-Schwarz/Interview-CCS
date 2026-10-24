import { ApplicationConfig } from '@angular/core';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';

import { provideApiConfiguration } from './api';
import { routes } from './app.routes';
import { authInterceptor } from './core/auth.interceptor';
import { environment } from '../environments/environment';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withFetch(), withInterceptors([authInterceptor])),
    // URL base del cliente generado (única configuración del front hacia la API).
    provideApiConfiguration(environment.apiBaseUrl)
  ]
};
