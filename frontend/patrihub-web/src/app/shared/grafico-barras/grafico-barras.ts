import {
  Component,
  DestroyRef,
  effect,
  inject,
  input,
  viewChild,
  type ElementRef,
} from '@angular/core';
import { Chart } from 'chart.js/auto';

/**
 * Gráfico de barras simples e genérico (rótulo + valor) — complementa a tabela detalhada de uma
 * feature, não a substitui (comparação visual rápida além dos números lado a lado). Barra com
 * valor negativo usa a cor de sinal negativo, mesma convenção de `.valor-positivo`/
 * `.valor-negativo` (DESIGN-SYSTEM.md §Cor) — cor com sinal só em valor com sinal, nunca
 * decorativa.
 */
@Component({
  selector: 'app-grafico-barras',
  template: `<canvas #canvas role="img" [attr.aria-label]="titulo()"></canvas>`,
  styles: `
    :host {
      display: block;
      position: relative;
      height: 18rem;
    }
  `,
})
export class GraficoBarras {
  readonly titulo = input.required<string>();
  readonly rotulos = input.required<string[]>();
  readonly valores = input.required<number[]>();

  private readonly canvasRef = viewChild<ElementRef<HTMLCanvasElement>>('canvas');
  private chart: Chart | null = null;

  constructor() {
    effect(() => {
      const canvas = this.canvasRef();
      const rotulos = this.rotulos();
      const valores = this.valores();
      const titulo = this.titulo();
      if (!canvas) {
        return;
      }
      this.desenhar(canvas.nativeElement, rotulos, valores, titulo);
    });

    inject(DestroyRef).onDestroy(() => this.chart?.destroy());
  }

  private desenhar(
    canvas: HTMLCanvasElement,
    rotulos: string[],
    valores: number[],
    titulo: string,
  ): void {
    this.chart?.destroy();

    const estilos = getComputedStyle(document.documentElement);
    const corPositivo = estilos.getPropertyValue('--sinal-pos').trim();
    const corNegativo = estilos.getPropertyValue('--sinal-neg').trim();
    const corTexto = estilos.getPropertyValue('--text-muted').trim();
    const corGrade = estilos.getPropertyValue('--border').trim();

    this.chart = new Chart(canvas, {
      type: 'bar',
      data: {
        labels: rotulos,
        datasets: [
          {
            data: valores,
            backgroundColor: valores.map((valor) => (valor >= 0 ? corPositivo : corNegativo)),
            borderRadius: 6,
            maxBarThickness: 48,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          title: { display: true, text: titulo, color: corTexto, font: { weight: 'bold' } },
        },
        scales: {
          x: { ticks: { color: corTexto }, grid: { display: false } },
          y: { ticks: { color: corTexto }, grid: { color: corGrade } },
        },
      },
    });
  }
}
