import { DecimalPipe, PercentPipe } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';

import { GraficoBarras } from '../../shared/grafico-barras/grafico-barras';
import { Dashboard } from '../dashboard';

/**
 * Visão consolidada do patrimônio: totais do mês/acumulado, um gráfico de barras de lucro
 * acumulado por Ativo (comparação visual rápida) e a tabela de métricas por Ativo lado a lado
 * (AC "permite comparar o desempenho entre os Ativos") com todos os números — o gráfico
 * complementa a tabela, não a substitui.
 */
@Component({
  selector: 'app-dashboard-pagina',
  imports: [ReactiveFormsModule, DecimalPipe, PercentPipe, GraficoBarras],
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

  /** Dados do gráfico de lucro acumulado — derivados de `dashboard.dado()`, mesma ordem da tabela. */
  protected readonly rotulosAtivos = computed(
    () => this.dashboard.dado()?.ativos.map((a) => a.apelido) ?? [],
  );
  protected readonly lucrosAcumulados = computed(
    () => this.dashboard.dado()?.ativos.map((a) => a.lucroAcumulado) ?? [],
  );

  constructor() {
    this.dashboard.carregar();
  }

  protected recalcular(): void {
    const valor = this.form.controls.taxaReferenciaAnualPercentual.value.trim();
    this.dashboard.carregar(valor === '' ? undefined : Number(valor) / 100);
  }
}
