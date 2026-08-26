import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

import { Auth } from '../auth/auth';

/**
 * Bloqueia rota protegida sem JWT em `localStorage`, redirecionando pro login (ver
 * `docs/spec/02-PLANO-TECNICO.md §8`). Só checa presença do token, não decodifica
 * expiração/assinatura no cliente — um token presente porém expirado passa o guard e só é
 * pego na primeira chamada à API seguinte, via o 401 tratado pelo `authInterceptor`. Decisão
 * deliberada (evita duplicar em JS a validação que o backend já faz) — ver "simples antes de
 * completo" em `docs/spec/00-CONSTITUTION.md`.
 */
export const authGuard: CanActivateFn = () => {
  const auth = inject(Auth);
  const router = inject(Router);

  if (auth.estaAutenticado()) {
    return true;
  }

  return router.createUrlTree(['/login']);
};
