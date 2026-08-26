import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { catchError, of } from 'rxjs';

import { environment } from '../../environments/environment';
import type { LancamentoDto, LancamentoFiltro, LancamentoRequest } from './lancamentos.models';

/**
 * CRUD de Lançamentos financeiros — mesmo padrão do service `Ativos` (Signal `lista`/
 * `carregando` + métodos HTTP diretos pras outras operações). `carregarLista` aceita um
 * `LancamentoFiltro` opcional, que vira query string em `GET /api/lancamentos` (ver
 * `LancamentosController.Listar`).
 */
@Injectable({
  providedIn: 'root',
})
export class Lancamentos {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/lancamentos`;

  private readonly listaSignal = signal<LancamentoDto[]>([]);
  private readonly carregandoSignal = signal(false);

  readonly lista = this.listaSignal.asReadonly();
  readonly carregando = this.carregandoSignal.asReadonly();

  carregarLista(filtro: LancamentoFiltro = {}): void {
    this.carregandoSignal.set(true);

    this.http
      .get<LancamentoDto[]>(this.baseUrl, { params: paramsDoFiltro(filtro) })
      .pipe(
        catchError(() => {
          this.carregandoSignal.set(false);
          return of(null);
        }),
      )
      .subscribe((lancamentos) => {
        if (lancamentos !== null) {
          this.listaSignal.set(lancamentos);
          this.carregandoSignal.set(false);
        }
      });
  }

  obterDetalhe(id: string) {
    return this.http.get<LancamentoDto>(`${this.baseUrl}/${id}`);
  }

  criar(request: LancamentoRequest) {
    return this.http.post<LancamentoDto>(this.baseUrl, request);
  }

  atualizar(id: string, request: LancamentoRequest) {
    return this.http.put<LancamentoDto>(`${this.baseUrl}/${id}`, request);
  }

  excluir(id: string) {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}

function paramsDoFiltro(filtro: LancamentoFiltro): Record<string, string> {
  const params: Record<string, string> = {};

  if (filtro.ativoId) {
    params['ativoId'] = filtro.ativoId;
  }
  if (filtro.dataInicio) {
    params['dataInicio'] = filtro.dataInicio;
  }
  if (filtro.dataFim) {
    params['dataFim'] = filtro.dataFim;
  }
  if (filtro.tipo !== undefined) {
    params['tipo'] = String(filtro.tipo);
  }

  return params;
}
