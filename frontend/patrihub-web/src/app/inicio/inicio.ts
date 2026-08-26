import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

import { Auth } from '../core/auth/auth';

/**
 * Landing page pós-login, provisória — vira o Dashboard consolidado na issue #22
 * (`docs/spec/02-PLANO-TECNICO.md §8`). Só existe aqui pra dar um destino protegido por
 * `authGuard` pro scaffolding de Auth ter o que mostrar, e agora linka pra listagem de Ativos
 * (issue #19).
 */
@Component({
  selector: 'app-inicio',
  imports: [RouterLink],
  templateUrl: './inicio.html',
  styleUrl: './inicio.css',
})
export class Inicio {
  protected readonly auth = inject(Auth);
}
