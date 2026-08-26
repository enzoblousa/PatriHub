import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { catchError, of } from 'rxjs';

import { environment } from '../../environments/environment';
import type { LocatarioDto, LocatarioRequest } from './locatarios.models';

/**
 * CRUD de Locatários — mesmo padrão dos services `Ativos`/`Lancamentos` (Signal `lista`/
 * `carregando` + métodos HTTP diretos). Sem `excluir`: o backend não expõe
 * `DELETE /api/locatarios/{id}` (ver `LocatariosController`).
 */
@Injectable({
  providedIn: 'root',
})
export class Locatarios {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/locatarios`;

  private readonly listaSignal = signal<LocatarioDto[]>([]);
  private readonly carregandoSignal = signal(false);

  readonly lista = this.listaSignal.asReadonly();
  readonly carregando = this.carregandoSignal.asReadonly();

  carregarLista(): void {
    this.carregandoSignal.set(true);

    this.http
      .get<LocatarioDto[]>(this.baseUrl)
      .pipe(
        catchError(() => {
          this.carregandoSignal.set(false);
          return of(null);
        }),
      )
      .subscribe((locatarios) => {
        if (locatarios !== null) {
          this.listaSignal.set(locatarios);
          this.carregandoSignal.set(false);
        }
      });
  }

  obterDetalhe(id: string) {
    return this.http.get<LocatarioDto>(`${this.baseUrl}/${id}`);
  }

  criar(request: LocatarioRequest) {
    return this.http.post<LocatarioDto>(this.baseUrl, request);
  }

  atualizar(id: string, request: LocatarioRequest) {
    return this.http.put<LocatarioDto>(`${this.baseUrl}/${id}`, request);
  }
}
