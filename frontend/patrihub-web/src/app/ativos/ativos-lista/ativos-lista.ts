import { DecimalPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

import { Ativos } from '../ativos';
import { ROTULOS_STATUS, ROTULOS_TIPO } from '../ativos-rotulos';
import type { StatusAtivo, TipoAtivo } from '../ativos.models';

/** Lista os Ativos do usuário autenticado (tipo, status, lucro do mês) via `GET /api/ativos`. */
@Component({
  selector: 'app-ativos-lista',
  imports: [RouterLink, DecimalPipe],
  templateUrl: './ativos-lista.html',
  styleUrl: './ativos-lista.css',
})
export class AtivosLista {
  protected readonly ativos = inject(Ativos);

  constructor() {
    this.ativos.carregarLista();
  }

  protected rotuloTipo(tipo: TipoAtivo): string {
    return ROTULOS_TIPO[tipo];
  }

  protected rotuloStatus(status: StatusAtivo): string {
    return ROTULOS_STATUS[status];
  }
}
