import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  let token;
  if(typeof localStorage !== 'undefined'){
    token = localStorage.getItem('accessToken');
  }

  if (token) {
    return true;
  }
  router.navigate(['/login'])
  return false;

};
