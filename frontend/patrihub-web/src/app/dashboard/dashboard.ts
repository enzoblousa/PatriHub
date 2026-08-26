import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { catchError, of } from 'rxjs';

import { environment } from '../../environments/environment';
import type { PatrimonioConsolidadoDto } from './dashboard.models';

/**
 * Visão consolidada do patrimônio — mesmo padrão dos demais services de feature (Signal
 * `dado`/`carregando`), mas sem lista própria: o "recurso" é sempre o dashboard inteiro do
 * usuário, recalculado a cada chamada (ver `IDashboardService`, nunca persistido).
 */
@Injectable({
  providedIn: 'root',
})
export class Dashboard {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/dashboard`;

  private readonly dadoSignal = signal<PatrimonioConsolidadoDto | null>(null);
  private readonly carregandoSignal = signal(false);

  readonly dado = this.dadoSignal.asReadonly();
  readonly carregando = this.carregandoSignal.asReadonly();

  /**
   * @param taxaReferenciaAnual Fração anual (ex.: 0.12 pra 12%); omitida, `CustoDeOportunidade`
   * vem `null` em cada Ativo (ver `DashboardController.Obter`).
   */
  carregar(taxaReferenciaAnual?: number): void {
    this.carregandoSignal.set(true);

    const params: Record<string, string> = {};
    if (taxaReferenciaAnual !== undefined) {
      params['taxaReferenciaAnual'] = String(taxaReferenciaAnual);
    }

    this.http
      .get<PatrimonioConsolidadoDto>(this.baseUrl, { params })
      .pipe(
        catchError(() => {
          this.carregandoSignal.set(false);
          return of(null);
        }),
      )
      .subscribe((dado) => {
        if (dado !== null) {
          this.dadoSignal.set(dado);
          this.carregandoSignal.set(false);
        }
      });
  }
}
