import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

/** Adjunta el JWT a cada petición y cierra la sesión si la API responde 401. */
export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);
  const session = auth.user();
  const isLoginRequest = request.url.endsWith('/api/auth/login');

  const authorized =
    session && !isLoginRequest
      ? request.clone({ setHeaders: { Authorization: `Bearer ${session.token}` } })
      : request;

  return next(authorized).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !isLoginRequest) {
        auth.logout();
      }
      return throwError(() => error);
    })
  );
};
