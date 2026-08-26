import { DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { Ativos } from '../../ativos/ativos';
import { mensagemErroHttp } from '../../core/http/mensagem-erro-http';
import { Locatarios } from '../../locatarios/locatarios';
import { Contratos } from '../contratos';
import { ROTULOS_STATUS } from '../contratos-rotulos';
import { StatusContrato } from '../contratos.models';

/**
 * Lista os Contratos do usuário com Status visível (`Ativo`/`Encerrado`/`Inadimplente`) via
 * `GET /api/contratos`. Encerrar pede confirmação inline antes do `POST .../encerrar`, mesmo
 * padrão de `AtivoDetalhe`/`LancamentosLista` — encerrar é irreversível (reverte o Ativo pra
 * Vago e não pode ser desfeito, ver `Contrato.Encerrar`).
 */
@Component({
  selector: 'app-contratos-lista',
  imports: [RouterLink, DecimalPipe],
  templateUrl: './contratos-lista.html',
  styleUrl: './contratos-lista.css',
})
export class ContratosLista {
  protected readonly contratos = inject(Contratos);
  protected readonly ativos = inject(Ativos);
  protected readonly locatarios = inject(Locatarios);

  protected readonly StatusContrato = StatusContrato;
  protected readonly rotulosStatus = ROTULOS_STATUS;

  protected readonly erro = signal<string | null>(null);
  protected readonly encerrandoId = signal<string | null>(null);
  protected readonly confirmandoEncerramentoId = signal<string | null>(null);

  constructor() {
    this.contratos.carregarLista();
    this.ativos.carregarLista();
    this.locatarios.carregarLista();
  }

  protected apelidoDoAtivo(ativoId: string): string {
    return this.ativos.lista().find((a) => a.id === ativoId)?.apelido ?? ativoId;
  }

  protected nomeDoLocatario(locatarioId: string): string {
    return this.locatarios.lista().find((l) => l.id === locatarioId)?.nome ?? locatarioId;
  }

  protected iniciarEncerramento(id: string): void {
    this.confirmandoEncerramentoId.set(id);
  }

  protected cancelarEncerramento(): void {
    this.confirmandoEncerramentoId.set(null);
  }

  protected confirmarEncerramento(id: string): void {
    this.encerrandoId.set(id);
    this.erro.set(null);

    this.contratos.encerrar(id).subscribe({
      next: () => {
        this.encerrandoId.set(null);
        this.confirmandoEncerramentoId.set(null);
        this.contratos.carregarLista();
        this.ativos.carregarLista();
      },
      error: (erro: unknown) => {
        this.encerrandoId.set(null);
        this.confirmandoEncerramentoId.set(null);
        this.erro.set(mensagemErroHttp(erro, 'Não foi possível encerrar este Contrato.'));
      },
    });
  }
}
