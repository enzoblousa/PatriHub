import { Component, computed, effect, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { Ativos } from '../../ativos/ativos';
import { mensagemErroHttp } from '../../core/http/mensagem-erro-http';
import { Lancamentos } from '../lancamentos';
import { categoriasPermitidas, ROTULOS_CATEGORIA, ROTULOS_TIPO } from '../lancamentos-categorias';
import type { LancamentoRequest } from '../lancamentos.models';
import { TipoLancamento } from '../lancamentos.models';

/**
 * Lança e edita um Lançamento na mesma tela: com `id` na rota (`/lancamentos/:id/editar`) faz
 * PUT; sem `id` (`/lancamentos/novo`) faz POST — mesmo padrão de `AtivoFormImovel`. O Ativo
 * pode vir pré-selecionado por `?ativoId=` (link "Novo Lançamento" na tela de detalhe do
 * Ativo).
 */
@Component({
  selector: 'app-lancamento-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './lancamento-form.html',
  styleUrl: './lancamento-form.css',
})
export class LancamentoForm {
  private readonly lancamentos = inject(Lancamentos);
  protected readonly ativos = inject(Ativos);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly formBuilder = inject(FormBuilder);

  protected readonly TipoLancamento = TipoLancamento;
  protected readonly rotulosTipo = ROTULOS_TIPO;
  protected readonly rotulosCategoria = ROTULOS_CATEGORIA;

  protected readonly lancamentoId = this.route.snapshot.paramMap.get('id');
  protected readonly modoEdicao = this.lancamentoId !== null;

  protected readonly carregando = signal(this.modoEdicao);
  protected readonly enviando = signal(false);
  protected readonly erro = signal<string | null>(null);

  protected readonly form = this.formBuilder.nonNullable.group({
    ativoId: [this.route.snapshot.queryParamMap.get('ativoId') ?? '', Validators.required],
    tipo: [TipoLancamento.Receita, Validators.required],
    categoria: [categoriasPermitidas(TipoLancamento.Receita)[0], Validators.required],
    valor: [0, [Validators.required, Validators.min(0.01)]],
    data: ['', Validators.required],
    descricao: [''],
  });

  private readonly tipoSignal = toSignal(this.form.controls.tipo.valueChanges, {
    initialValue: this.form.controls.tipo.getRawValue(),
  });
  protected readonly categoriasDisponiveis = computed(() => categoriasPermitidas(this.tipoSignal()));

  constructor() {
    this.ativos.carregarLista();

    effect(() => {
      const disponiveis = this.categoriasDisponiveis();
      if (!disponiveis.includes(this.form.controls.categoria.value)) {
        this.form.controls.categoria.setValue(disponiveis[0]);
      }
    });

    if (this.lancamentoId !== null) {
      this.lancamentos.obterDetalhe(this.lancamentoId).subscribe({
        next: (lancamento) => {
          this.form.patchValue({
            ativoId: lancamento.ativoId,
            tipo: lancamento.tipo,
            categoria: lancamento.categoria,
            valor: lancamento.valor,
            data: lancamento.data,
            descricao: lancamento.descricao ?? '',
          });
          this.carregando.set(false);
        },
        error: () => {
          this.carregando.set(false);
          this.erro.set('Não foi possível carregar este Lançamento.');
        },
      });
    }
  }

  protected salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.enviando.set(true);
    this.erro.set(null);

    const valores = this.form.getRawValue();
    const request: LancamentoRequest = {
      ...valores,
      descricao: valores.descricao.trim() === '' ? null : valores.descricao.trim(),
      contratoId: null,
    };

    const requisicao =
      this.lancamentoId !== null
        ? this.lancamentos.atualizar(this.lancamentoId, request)
        : this.lancamentos.criar(request);

    requisicao.subscribe({
      next: () => this.router.navigate(['/lancamentos'], { queryParams: { ativoId: request.ativoId } }),
      error: (erro: unknown) => {
        this.enviando.set(false);
        this.erro.set(mensagemErroHttp(erro, 'Não foi possível salvar o Lançamento. Tente novamente.'));
      },
    });
  }
}
