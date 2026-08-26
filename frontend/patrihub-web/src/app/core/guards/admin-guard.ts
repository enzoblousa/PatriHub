import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

import { Auth } from '../auth/auth';

/**
 * Exige a claim de Role `Admin` no token (ver `PatriHubClaimTypes`/`ClaimTypes.Role` no
 * backend), além de sessão válida — usado pelas rotas do backoffice de Admin. Redireciona
 * pro login quando barra, mesmo comportamento do `authGuard` (ver
 * `docs/spec/02-PLANO-TECNICO.md §8`).
 */
export const adminGuard: CanActivateFn = () => {
  const auth = inject(Auth);
  const router = inject(Router);

  if (auth.estaAutenticado() && auth.usuario()?.papel === 'Admin') {
    return true;
  }

  return router.createUrlTree(['/login']);
};
