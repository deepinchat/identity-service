import { CanActivateFn, Router } from '@angular/router';
import { AccountService } from '../services/account.service';
import { inject } from '@angular/core';
import { map } from 'rxjs';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AccountService);
  const router = inject(Router);
  return authService.isAuthorized().pipe(map(isAuthorized => {
    if (!isAuthorized) {
      router.navigate(['/signin'], { queryParams: { returnUrl: state.url } });
    }
    return isAuthorized;
  }));
};
