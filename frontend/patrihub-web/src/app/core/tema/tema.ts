import { Injectable, signal } from '@angular/core';

export type NomeTema = 'escuro' | 'claro';

const CHAVE_TEMA = 'patrihub.tema';
const ATRIBUTO_TEMA = 'data-theme';

/**
 * Escuro é o tema base do sistema de design (ver docs/design/DESIGN-SYSTEM.md) — claro é a
 * alternativa, não o padrão. Persistido em `localStorage` (mesmo padrão de `Auth`) e aplicado
 * como `data-theme` em `<html>`, lido pelas variáveis de `styles.css`. `index.html` tem um
 * script inline que já aplica o tema salvo antes do Angular subir, pra não piscar dark→light
 * numa sessão que prefere claro.
 */
@Injectable({
  providedIn: 'root',
})
export class Tema {
  private readonly temaSignal = signal<NomeTema>(lerTemaArmazenado());

  readonly tema = this.temaSignal.asReadonly();

  constructor() {
    aplicarNoDocumento(this.temaSignal());
  }

  alternar(): void {
    const novo: NomeTema = this.temaSignal() === 'escuro' ? 'claro' : 'escuro';
    this.temaSignal.set(novo);
    localStorage.setItem(CHAVE_TEMA, novo);
    aplicarNoDocumento(novo);
  }
}

function lerTemaArmazenado(): NomeTema {
  return localStorage.getItem(CHAVE_TEMA) === 'claro' ? 'claro' : 'escuro';
}

function aplicarNoDocumento(tema: NomeTema): void {
  document.documentElement.setAttribute(ATRIBUTO_TEMA, tema);
}
