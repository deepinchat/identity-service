import { HttpInterceptorFn } from '@angular/common/http';
import { catchError } from 'rxjs';
import { AccountService } from '../services/account.service';
import { inject } from '@angular/core';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const accountService = inject(AccountService);
  return next(req).pipe(catchError(err => {
    if (err.status === 401) {
      accountService.logout();
    }
    throw err;
  }));
};
