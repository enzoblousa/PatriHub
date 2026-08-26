import { DecimalPipe, PercentPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';

import { Dashboard } from '../dashboard';

/**
 * Visão consolidada do patrimônio: totais do mês/acumulado e a tabela de métricas por Ativo
 * lado a lado (AC "permite comparar o desempenho entre os Ativos") — uma única tabela com
 * todos os Ativos já serve de comparação, sem necessidade de gráfico/ordenação extra (ver
 * "simples antes de completo" em `docs/spec/00-CONSTITUTION.md`).
 */
@Component({
  selector: 'app-dashboard-pagina',
  imports: [ReactiveFormsModule, DecimalPipe, PercentPipe],
  templateUrl: './dashboard-pagina.html',
  styleUrl: './dashboard-pagina.css',
})
export class DashboardPagina {
  protected readonly dashboard = inject(Dashboard);
  private readonly formBuilder = inject(FormBuilder);

  /** Digitada como percentual (ex.: "12"), convertida pra fração (0.12) antes de mandar pra API. */
  protected readonly form = this.formBuilder.nonNullable.group({
    taxaReferenciaAnualPercentual: [''],
  });

  constructor() {
    this.dashboard.carregar();
  }

  protected recalcular(): void {
    const valor = this.form.controls.taxaReferenciaAnualPercentual.value.trim();
    this.dashboard.carregar(valor === '' ? undefined : Number(valor) / 100);
  }
}
