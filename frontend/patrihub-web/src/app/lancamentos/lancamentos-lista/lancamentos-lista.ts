import { DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { Ativos } from '../../ativos/ativos';
import { mensagemErroHttp } from '../../core/http/mensagem-erro-http';
import { buscarRotulo } from '../../core/util/buscar-rotulo';
import { Lancamentos } from '../lancamentos';
import { ROTULOS_CATEGORIA, ROTULOS_TIPO } from '../lancamentos-categorias';
import type { LancamentoFiltro } from '../lancamentos.models';
import { TipoLancamento } from '../lancamentos.models';

/**
 * Lista/filtra Lançamentos por Ativo, período e tipo (`GET /api/lancamentos`). Exclusão pede
 * confirmação inline antes do `DELETE`, mesmo padrão de `AtivoDetalhe`.
 */
@Component({
  selector: 'app-lancamentos-lista',
  imports: [RouterLink, ReactiveFormsModule, DecimalPipe],
  templateUrl: './lancamentos-lista.html',
  styleUrl: './lancamentos-lista.css',
})
export class LancamentosLista {
  protected readonly lancamentos = inject(Lancamentos);
  protected readonly ativos = inject(Ativos);
  private readonly route = inject(ActivatedRoute);
  private readonly formBuilder = inject(FormBuilder);

  protected readonly TipoLancamento = TipoLancamento;
  protected readonly rotulosTipo = ROTULOS_TIPO;
  protected readonly rotulosCategoria = ROTULOS_CATEGORIA;

  protected readonly erro = signal<string | null>(null);
  protected readonly excluindoId = signal<string | null>(null);
  protected readonly confirmandoExclusaoId = signal<string | null>(null);

  protected readonly filtro = this.formBuilder.nonNullable.group({
    ativoId: this.route.snapshot.queryParamMap.get('ativoId') ?? '',
    dataInicio: '',
    dataFim: '',
    tipo: '',
  });

  constructor() {
    this.ativos.carregarLista();
    this.filtrar();
  }

  protected apelidoDoAtivo(ativoId: string): string {
    return buscarRotulo(this.ativos.lista(), ativoId, (a) => a.apelido);
  }

  protected filtrar(): void {
    const valores = this.filtro.getRawValue();
    const filtro: LancamentoFiltro = {
      ativoId: valores.ativoId || undefined,
      dataInicio: valores.dataInicio || undefined,
      dataFim: valores.dataFim || undefined,
      tipo: valores.tipo === '' ? undefined : (Number(valores.tipo) as TipoLancamento),
    };
    this.lancamentos.carregarLista(filtro);
  }

  protected limparFiltro(): void {
    this.filtro.reset({ ativoId: '', dataInicio: '', dataFim: '', tipo: '' });
    this.filtrar();
  }

  protected iniciarExclusao(id: string): void {
    this.confirmandoExclusaoId.set(id);
  }

  protected cancelarExclusao(): void {
    this.confirmandoExclusaoId.set(null);
  }

  protected confirmarExclusao(id: string): void {
    this.excluindoId.set(id);
    this.erro.set(null);

    this.lancamentos.excluir(id).subscribe({
      next: () => {
        this.excluindoId.set(null);
        this.confirmandoExclusaoId.set(null);
        this.filtrar();
      },
      error: (erro: unknown) => {
        this.excluindoId.set(null);
        this.confirmandoExclusaoId.set(null);
        this.erro.set(mensagemErroHttp(erro, 'Não foi possível excluir este Lançamento.'));
      },
    });
  }
}
