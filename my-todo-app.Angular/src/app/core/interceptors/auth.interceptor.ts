import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

const PUBLIC_ENDPOINTS = ['/auth/Login', '/auth/Register'];

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const isPublic = PUBLIC_ENDPOINTS.some(path => req.url.includes(path));
  if (isPublic) return next(req);

  const token = inject(AuthService).token();
  if (token) {
    req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
  }
  return next(req);
};
