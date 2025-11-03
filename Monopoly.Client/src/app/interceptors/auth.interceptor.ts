import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  
  // Get token from localStorage
  const token = localStorage.getItem('authToken');
  
  // If token exists, clone request and add Authorization header
  if (token) {
    const clonedRequest = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    
    return next(clonedRequest).pipe(
      catchError((error) => {
        // If 401 Unauthorized, clear localStorage and redirect to login
        if (error.status === 401) {
          localStorage.removeItem('authToken');
          localStorage.removeItem('username');
          router.navigate(['/login']);
        }
        return throwError(() => error);
      })
    );
  }
  
  // If no token, proceed with original request
  return next(req);
};
