import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError, timeout, TimeoutError } from 'rxjs';
import { MessageService } from 'primeng/api';
import { AuthService } from '../services/auth.service';

const REQUEST_TIMEOUT_MS = 15_000;
let handlingUnauth = false;

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const auth     = inject(AuthService);
  const messages = inject(MessageService);

  return next(req).pipe(
    timeout(REQUEST_TIMEOUT_MS),
    catchError(err => {
      if (err instanceof TimeoutError) {
        messages.add({
          severity: 'warn',
          summary: 'Request Timed Out',
          detail: 'The server is taking too long to respond. Please try again.',
          life: 5000,
        });
        return throwError(() => err);
      }

      if (err instanceof HttpErrorResponse) {
        if (err.status === 0) {
          messages.add({
            severity: 'error',
            summary: 'Cannot Reach Server',
            detail: 'Check that the API is running and try again.',
            life: 5000,
          });
        } else if (err.status === 401 && !handlingUnauth) {
          handlingUnauth = true;
          messages.add({
            severity: 'warn',
            summary: 'Session Expired',
            detail: 'Your session has expired or been revoked. Please log in again.',
            life: 4000,
          });
          setTimeout(() => { auth.clearLocal(); handlingUnauth = false; }, 4000);
        } else {
          const detail = err.error?.message ?? err.error?.title ?? err.message ?? 'Something went wrong';
          messages.add({ severity: 'error', summary: `Error ${err.status}`, detail, life: 4000 });
        }
      }

      return throwError(() => err);
    })
  );
};
