import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { catchError, of } from 'rxjs';

import { environment } from '../../environments/environment';
import type { ContratoDto, ContratoRequest } from './contratos.models';

/**
 * Criação, encerramento e listagem de Contratos — mesmo padrão dos demais services de
 * feature (Signal `lista`/`carregando` + métodos HTTP diretos). Sem `atualizar`: não há
 * edição de Contrato no MVP (ver `ContratoRequest`), só criação e encerramento.
 */
@Injectable({
  providedIn: 'root',
})
export class Contratos {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/contratos`;

  private readonly listaSignal = signal<ContratoDto[]>([]);
  private readonly carregandoSignal = signal(false);

  readonly lista = this.listaSignal.asReadonly();
  readonly carregando = this.carregandoSignal.asReadonly();

  carregarLista(): void {
    this.carregandoSignal.set(true);

    this.http
      .get<ContratoDto[]>(this.baseUrl)
      .pipe(
        catchError(() => {
          this.carregandoSignal.set(false);
          return of(null);
        }),
      )
      .subscribe((contratos) => {
        if (contratos !== null) {
          this.listaSignal.set(contratos);
          this.carregandoSignal.set(false);
        }
      });
  }

  criar(request: ContratoRequest) {
    return this.http.post<ContratoDto>(this.baseUrl, request);
  }

  encerrar(id: string) {
    return this.http.post<ContratoDto>(`${this.baseUrl}/${id}/encerrar`, null);
  }
}
