import { DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { mensagemErroHttp } from '../../core/http/mensagem-erro-http';
import { Ajuda } from '../../shared/ajuda/ajuda';
import { Ativos } from '../ativos';
import {
  ROTULOS_MOTORIZACAO,
  ROTULOS_STATUS,
  ROTULOS_TIPO,
  TEXTOS_AJUDA_CARRO,
  TEXTOS_AJUDA_IMOVEL,
  UNIDADE_CONSUMO_MEDIO,
} from '../ativos-rotulos';
import type { AtivoDetalheDto } from '../ativos.models';
import { Motorizacao, StatusAtivo, TipoAtivo } from '../ativos.models';

/**
 * Detalhe completo de um Ativo (`GET /api/ativos/{id}`): permite marcar `Manutenção`/`À venda`
 * (único jeito manual de setar Status — ver `Ativo.MarcarStatusManual`) e excluir, com
 * confirmação inline antes do `DELETE` (evita `window.confirm`, que trava o layout e é difícil
 * de testar).
 */
@Component({
  selector: 'app-ativo-detalhe',
  imports: [RouterLink, Ajuda, DecimalPipe],
  templateUrl: './ativo-detalhe.html',
  styleUrl: './ativo-detalhe.css',
})
export class AtivoDetalhe {
  private readonly ativos = inject(Ativos);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly TipoAtivo = TipoAtivo;
  protected readonly StatusAtivo = StatusAtivo;
  protected readonly textosAjudaCarro = TEXTOS_AJUDA_CARRO;
  protected readonly textosAjudaImovel = TEXTOS_AJUDA_IMOVEL;

  protected readonly ativoId = this.route.snapshot.paramMap.get('id')!;
  protected readonly detalhe = signal<AtivoDetalheDto | null>(null);
  protected readonly carregando = signal(true);
  protected readonly erro = signal<string | null>(null);
  protected readonly atualizandoStatus = signal(false);
  protected readonly confirmandoExclusao = signal(false);
  protected readonly excluindo = signal(false);

  constructor() {
    this.carregar();
  }

  protected rotuloTipo(tipo: TipoAtivo): string {
    return ROTULOS_TIPO[tipo];
  }

  protected rotuloStatus(status: StatusAtivo): string {
    return ROTULOS_STATUS[status];
  }

  protected rotuloMotorizacao(motorizacao: Motorizacao): string {
    return ROTULOS_MOTORIZACAO[motorizacao];
  }

  protected unidadeConsumoMedio(motorizacao: Motorizacao): string {
    return UNIDADE_CONSUMO_MEDIO[motorizacao];
  }

  protected rotaEdicao(): string[] {
    const tipo = this.detalhe()?.tipo === TipoAtivo.Carro ? 'carros' : 'imoveis';
    return ['/ativos', tipo, this.ativoId, 'editar'];
  }

  protected marcarStatus(status: StatusAtivo): void {
    this.atualizandoStatus.set(true);
    this.erro.set(null);

    this.ativos.marcarStatus(this.ativoId, status).subscribe({
      next: (detalhe) => {
        this.detalhe.set(detalhe);
        this.atualizandoStatus.set(false);
      },
      error: (erro: unknown) => {
        this.atualizandoStatus.set(false);
        this.erro.set(mensagemErroHttp(erro, 'Não foi possível alterar o status.'));
      },
    });
  }

  protected iniciarExclusao(): void {
    this.confirmandoExclusao.set(true);
  }

  protected cancelarExclusao(): void {
    this.confirmandoExclusao.set(false);
  }

  protected confirmarExclusao(): void {
    this.excluindo.set(true);
    this.erro.set(null);

    this.ativos.excluir(this.ativoId).subscribe({
      next: () => this.router.navigateByUrl('/ativos'),
      error: (erro: unknown) => {
        this.excluindo.set(false);
        this.confirmandoExclusao.set(false);
        this.erro.set(mensagemErroHttp(erro, 'Não foi possível excluir este Ativo.'));
      },
    });
  }

  private carregar(): void {
    this.ativos.obterDetalhe(this.ativoId).subscribe({
      next: (detalhe) => {
        this.detalhe.set(detalhe);
        this.carregando.set(false);
      },
      error: (erro: unknown) => {
        this.carregando.set(false);
        this.erro.set(mensagemErroHttp(erro, 'Não foi possível carregar este Ativo.'));
      },
    });
  }
}
