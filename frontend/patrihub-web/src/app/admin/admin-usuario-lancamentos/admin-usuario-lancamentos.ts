import { DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';

import type { AtivoResumoDto } from '../../ativos/ativos.models';
import { mensagemErroHttp } from '../../core/http/mensagem-erro-http';
import { buscarRotulo } from '../../core/util/buscar-rotulo';
import { ROTULOS_CATEGORIA, ROTULOS_TIPO } from '../../lancamentos/lancamentos-categorias';
import type { LancamentoDto } from '../../lancamentos/lancamentos.models';
import { TipoLancamento } from '../../lancamentos/lancamentos.models';
import { Admin } from '../admin';

/**
 * Leitura auditada (ver ADR-0002), somente leitura. Carrega os Ativos do mesmo usuário junto
 * (`forkJoin`) só pra exibir o apelido em vez do id cru — mesma UX de `LancamentosLista`.
 */
@Component({
  selector: 'app-admin-usuario-lancamentos',
  imports: [RouterLink, DecimalPipe],
  templateUrl: './admin-usuario-lancamentos.html',
  styleUrl: './admin-usuario-lancamentos.css',
})
export class AdminUsuarioLancamentos {
  private readonly admin = inject(Admin);
  private readonly route = inject(ActivatedRoute);

  protected readonly usuarioId = this.route.snapshot.paramMap.get('id')!;
  protected readonly lancamentos = signal<LancamentoDto[]>([]);
  protected readonly ativos = signal<AtivoResumoDto[]>([]);
  protected readonly carregando = signal(true);
  protected readonly erro = signal<string | null>(null);

  protected readonly rotulosTipo = ROTULOS_TIPO;
  protected readonly rotulosCategoria = ROTULOS_CATEGORIA;
  protected readonly TipoLancamento = TipoLancamento;

  constructor() {
    forkJoin({
      lancamentos: this.admin.listarLancamentosDoUsuario(this.usuarioId),
      ativos: this.admin.listarAtivosDoUsuario(this.usuarioId),
    }).subscribe({
      next: ({ lancamentos, ativos }) => {
        this.lancamentos.set(lancamentos);
        this.ativos.set(ativos);
        this.carregando.set(false);
      },
      error: (erro: unknown) => {
        this.carregando.set(false);
        this.erro.set(
          mensagemErroHttp(erro, 'Não foi possível carregar os Lançamentos deste usuário.'),
        );
      },
    });
  }

  protected apelidoDoAtivo(ativoId: string): string {
    return buscarRotulo(this.ativos(), ativoId, (a) => a.apelido);
  }
}
