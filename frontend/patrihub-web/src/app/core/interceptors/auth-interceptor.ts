import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import { Auth } from '../auth/auth';

/**
 * Único ponto que injeta `Authorization: Bearer <token>` nas chamadas à API e trata 401 de
 * forma centralizada (limpa a sessão, redireciona pro login) — nenhum service de feature
 * repete essa lógica (ver `docs/spec/02-PLANO-TECNICO.md §8`).
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(Auth);

  const chamaApi = req.url.startsWith(environment.apiBaseUrl);
  const token = auth.obterToken();
  const requisicao =
    chamaApi && token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

  return next(requisicao).pipe(
    catchError((erro: unknown) => {
      if (chamaApi && erro instanceof HttpErrorResponse && erro.status === 401) {
        auth.logout();
      }

      return throwError(() => erro);
    }),
  );
};
