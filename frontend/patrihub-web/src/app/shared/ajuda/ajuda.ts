import { Component, type ElementRef, signal, viewChild, input } from '@angular/core';

let proximoId = 0;

/**
 * Ícone "?" ao lado de um rótulo/número cujo significado não é óbvio pra quem não conhece o
 * jargão financeiro (ex.: "ROI (FIPE)", "Consumo médio", "Motorização") — ao passar o mouse ou
 * focar via teclado, mostra uma explicação curta do que é aquilo e/ou o que preencher (issue #53).
 *
 * O balão é posicionado via `position: fixed` calculado em JS (`getBoundingClientRect`), não
 * `position: absolute` puro — este componente também é usado dentro de `<th>` na tabela do
 * dashboard, que tem `overflow: hidden` (cantos arredondados, ver `styles.css`); um balão
 * `absolute` seria cortado ali. `position: fixed` escapa desse corte porque é relativo à
 * viewport, não ao ancestral com overflow.
 *
 * Acessível por design, não só por hover: `aria-describedby` liga o botão ao texto de forma
 * permanente (o `<span>` do balão nunca sai do DOM, só fica com `opacity: 0` quando escondido —
 * nunca `display`/`visibility`, que tirariam o texto da árvore de acessibilidade). Cada
 * instância tem seu próprio id (`proximoId`) pra não colidir quando há vários na mesma tela.
 */
@Component({
  selector: 'app-ajuda',
  template: `
    <span class="ajuda">
      <button
        #botao
        type="button"
        class="ajuda-icone"
        [attr.aria-describedby]="id"
        aria-label="Ajuda"
        (mouseenter)="mostrar()"
        (mouseleave)="esconder()"
        (focus)="mostrar()"
        (blur)="esconder()"
      >
        ?
      </button>
      <span
        [id]="id"
        class="ajuda-balao"
        [class.ajuda-balao--visivel]="visivel()"
        [style.top.px]="posicao()?.top"
        [style.left.px]="posicao()?.left"
      >
        {{ texto() }}
      </span>
    </span>
  `,
  styleUrl: './ajuda.css',
})
export class Ajuda {
  readonly texto = input.required<string>();
  protected readonly id = `ajuda-${proximoId++}`;

  private readonly botao = viewChild.required<ElementRef<HTMLButtonElement>>('botao');
  protected readonly visivel = signal(false);
  protected readonly posicao = signal<{ top: number; left: number } | null>(null);

  protected mostrar(): void {
    const retangulo = this.botao().nativeElement.getBoundingClientRect();
    this.posicao.set({ top: retangulo.bottom + 6, left: retangulo.left + retangulo.width / 2 });
    this.visivel.set(true);
  }

  protected esconder(): void {
    this.visivel.set(false);
  }
}
