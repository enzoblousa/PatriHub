import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { catchError, of } from 'rxjs';

import { environment } from '../../environments/environment';
import type {
  AtivoDetalheDto,
  AtivoResumoDto,
  CarroRequest,
  ImovelRequest,
  StatusAtivo,
} from './ativos.models';

/**
 * CRUD de Ativos (Imóvel/Carro) — service único da feature, concentrando chamada HTTP + estado
 * (ver `docs/spec/02-PLANO-TECNICO.md §8`). Só `lista`/`carregando` viram Signal: as telas de
 * formulário e detalhe chamam a API diretamente e reagem ao Observable, sem precisar de estado
 * compartilhado.
 */
@Injectable({
  providedIn: 'root',
})
export class Ativos {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/ativos`;

  private readonly listaSignal = signal<AtivoResumoDto[]>([]);
  private readonly carregandoSignal = signal(false);

  readonly lista = this.listaSignal.asReadonly();
  readonly carregando = this.carregandoSignal.asReadonly();

  carregarLista(): void {
    this.carregandoSignal.set(true);

    this.http
      .get<AtivoResumoDto[]>(this.baseUrl)
      .pipe(
        catchError(() => {
          this.carregandoSignal.set(false);
          return of(null);
        }),
      )
      .subscribe((ativos) => {
        if (ativos !== null) {
          this.listaSignal.set(ativos);
          this.carregandoSignal.set(false);
        }
      });
  }

  obterDetalhe(id: string) {
    return this.http.get<AtivoDetalheDto>(`${this.baseUrl}/${id}`);
  }

  criarImovel(request: ImovelRequest) {
    return this.http.post<AtivoDetalheDto>(`${this.baseUrl}/imoveis`, request);
  }

  criarCarro(request: CarroRequest) {
    return this.http.post<AtivoDetalheDto>(`${this.baseUrl}/carros`, request);
  }

  atualizarImovel(id: string, request: ImovelRequest) {
    return this.http.put<AtivoDetalheDto>(`${this.baseUrl}/imoveis/${id}`, request);
  }

  atualizarCarro(id: string, request: CarroRequest) {
    return this.http.put<AtivoDetalheDto>(`${this.baseUrl}/carros/${id}`, request);
  }

  marcarStatus(id: string, status: StatusAtivo) {
    return this.http.patch<AtivoDetalheDto>(`${this.baseUrl}/${id}/status`, { status });
  }

  excluir(id: string) {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
