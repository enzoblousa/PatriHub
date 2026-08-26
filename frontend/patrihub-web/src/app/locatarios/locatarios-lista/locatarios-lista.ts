import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

import { Locatarios } from '../locatarios';

/** Lista os Locatários do usuário autenticado via `GET /api/locatarios`. */
@Component({
  selector: 'app-locatarios-lista',
  imports: [RouterLink],
  templateUrl: './locatarios-lista.html',
  styleUrl: './locatarios-lista.css',
})
export class LocatariosLista {
  protected readonly locatarios = inject(Locatarios);

  constructor() {
    this.locatarios.carregarLista();
  }
}
