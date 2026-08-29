import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { mensagemErroHttp } from '../../core/http/mensagem-erro-http';
import { Ajuda } from '../../shared/ajuda/ajuda';
import { Ativos } from '../ativos';
import { ROTULOS_MOTORIZACAO, UNIDADE_CONSUMO_MEDIO } from '../ativos-rotulos';
import type { CarroRequest } from '../ativos.models';
import { Motorizacao } from '../ativos.models';
import {
  criarFinanciamentoForm,
  financiamentoFormParaDto,
  preencherFinanciamentoForm,
} from '../financiamento-form';

/**
 * Cadastro e edição de Carro na mesma tela: com `id` na rota (`/ativos/carros/:id/editar`)
 * carrega o Ativo existente e faz PUT; sem `id` (`/ativos/carros/novo`) faz POST — mesmo
 * padrão de `AtivoFormImovel`.
 */
@Component({
  selector: 'app-ativo-form-carro',
  imports: [ReactiveFormsModule, RouterLink, Ajuda],
  templateUrl: './ativo-form-carro.html',
  styleUrl: './ativo-form-carro.css',
})
export class AtivoFormCarro {
  private readonly ativos = inject(Ativos);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly formBuilder = inject(FormBuilder);

  protected readonly ativoId = this.route.snapshot.paramMap.get('id');
  protected readonly modoEdicao = this.ativoId !== null;

  protected readonly carregando = signal(this.modoEdicao);
  protected readonly enviando = signal(false);
  protected readonly erro = signal<string | null>(null);
  protected readonly temFinanciamento = signal(false);

  protected readonly financiamentoForm = criarFinanciamentoForm(this.formBuilder);

  protected readonly Motorizacao = Motorizacao;
  protected readonly opcoesMotorizacao = [Motorizacao.Combustao, Motorizacao.Eletrico];
  protected readonly rotulosMotorizacao = ROTULOS_MOTORIZACAO;

  protected readonly form = this.formBuilder.nonNullable.group({
    apelido: ['', Validators.required],
    dataAquisicao: ['', Validators.required],
    valorAquisicao: [0, [Validators.required, Validators.min(0)]],
    valorMercadoAtual: [0, [Validators.required, Validators.min(0)]],
    placa: ['', Validators.required],
    marca: ['', Validators.required],
    modelo: ['', Validators.required],
    anoFabricacao: [new Date().getFullYear(), Validators.required],
    anoModelo: [new Date().getFullYear(), Validators.required],
    valorFipeAtual: [0, [Validators.required, Validators.min(0)]],
    km: [0, [Validators.required, Validators.min(0)]],
    motorizacao: [Motorizacao.Combustao, Validators.required],
    consumoMedio: [0, [Validators.required, Validators.min(0)]],
  });

  /** Unidade de leitura de `consumoMedio` depende da Motorização escolhida — ver CONTEXT.md. */
  protected unidadeConsumoMedio(): string {
    return UNIDADE_CONSUMO_MEDIO[this.form.controls.motorizacao.value];
  }

  constructor() {
    if (this.ativoId !== null) {
      this.ativos.obterDetalhe(this.ativoId).subscribe({
        next: (detalhe) => {
          this.form.patchValue({
            apelido: detalhe.apelido,
            dataAquisicao: detalhe.dataAquisicao,
            valorAquisicao: detalhe.valorAquisicao,
            valorMercadoAtual: detalhe.valorMercadoAtual,
            placa: detalhe.carro?.placa,
            marca: detalhe.carro?.marca,
            modelo: detalhe.carro?.modelo,
            anoFabricacao: detalhe.carro?.anoFabricacao,
            anoModelo: detalhe.carro?.anoModelo,
            valorFipeAtual: detalhe.carro?.valorFipeAtual,
            km: detalhe.carro?.km,
            motorizacao: detalhe.carro?.motorizacao,
            consumoMedio: detalhe.carro?.consumoMedio,
          });
          if (detalhe.financiamento) {
            this.temFinanciamento.set(true);
            preencherFinanciamentoForm(this.financiamentoForm, detalhe.financiamento);
          }
          this.carregando.set(false);
        },
        error: () => {
          this.carregando.set(false);
          this.erro.set('Não foi possível carregar este Carro.');
        },
      });
    }
  }

  protected salvar(): void {
    if (this.form.invalid || (this.temFinanciamento() && this.financiamentoForm.invalid)) {
      this.form.markAllAsTouched();
      this.financiamentoForm.markAllAsTouched();
      return;
    }

    this.enviando.set(true);
    this.erro.set(null);

    const request: CarroRequest = {
      ...this.form.getRawValue(),
      financiamento: financiamentoFormParaDto(this.financiamentoForm, this.temFinanciamento()),
    };

    const requisicao =
      this.ativoId !== null
        ? this.ativos.atualizarCarro(this.ativoId, request)
        : this.ativos.criarCarro(request);

    requisicao.subscribe({
      next: (detalhe) => this.router.navigate(['/ativos', detalhe.id]),
      error: (erro: unknown) => {
        this.enviando.set(false);
        this.erro.set(mensagemErroHttp(erro, 'Não foi possível salvar o Carro. Tente novamente.'));
      },
    });
  }
}
