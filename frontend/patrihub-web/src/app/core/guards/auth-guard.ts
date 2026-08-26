import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

import { Auth } from '../auth/auth';

/**
 * Bloqueia rota protegida sem JWT válido em `localStorage`, redirecionando pro login (ver
 * `docs/spec/02-PLANO-TECNICO.md §8`).
 */
export const authGuard: CanActivateFn = () => {
  const auth = inject(Auth);
  const router = inject(Router);

  if (auth.estaAutenticado()) {
    return true;
  }

  return router.createUrlTree(['/login']);
};
