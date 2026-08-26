import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { Ativos } from '../ativos';
import { mensagemErroAtivo } from '../ativos-erro-http';
import type { ImovelRequest } from '../ativos.models';
import { TipoImovel } from '../ativos.models';
import {
  criarFinanciamentoForm,
  financiamentoFormParaDto,
  preencherFinanciamentoForm,
} from '../financiamento-form';

/**
 * Cadastro e edição de Imóvel na mesma tela: com `id` na rota (`/ativos/imoveis/:id/editar`)
 * carrega o Ativo existente e faz PUT; sem `id` (`/ativos/imoveis/novo`) faz POST. Evita
 * duplicar o formulário entre as duas telas (mesmos campos exigidos nos dois casos — ver
 * `ImovelRequest`).
 */
@Component({
  selector: 'app-ativo-form-imovel',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './ativo-form-imovel.html',
  styleUrl: './ativo-form-imovel.css',
})
export class AtivoFormImovel {
  private readonly ativos = inject(Ativos);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly formBuilder = inject(FormBuilder);

  protected readonly tiposImovel = [
    { valor: TipoImovel.Apartamento, rotulo: 'Apartamento' },
    { valor: TipoImovel.Casa, rotulo: 'Casa' },
    { valor: TipoImovel.Comercial, rotulo: 'Comercial' },
    { valor: TipoImovel.Terreno, rotulo: 'Terreno' },
  ];

  protected readonly ativoId = this.route.snapshot.paramMap.get('id');
  protected readonly modoEdicao = this.ativoId !== null;

  protected readonly carregando = signal(this.modoEdicao);
  protected readonly enviando = signal(false);
  protected readonly erro = signal<string | null>(null);
  protected readonly temFinanciamento = signal(false);

  protected readonly financiamentoForm = criarFinanciamentoForm(this.formBuilder);

  protected readonly form = this.formBuilder.nonNullable.group({
    apelido: ['', Validators.required],
    dataAquisicao: ['', Validators.required],
    valorAquisicao: [0, [Validators.required, Validators.min(0)]],
    valorMercadoAtual: [0, [Validators.required, Validators.min(0)]],
    tipoImovel: [TipoImovel.Apartamento, Validators.required],
    areaM2: [0, [Validators.required, Validators.min(0.01)]],
    matricula: ['', Validators.required],
    valorIptuMensal: [0, [Validators.required, Validators.min(0)]],
    valorCondominioMensal: [0, [Validators.required, Validators.min(0)]],
    endereco: this.formBuilder.nonNullable.group({
      rua: ['', Validators.required],
      numero: ['', Validators.required],
      complemento: [''],
      bairro: ['', Validators.required],
      cidade: ['', Validators.required],
      uf: ['', [Validators.required, Validators.maxLength(2), Validators.minLength(2)]],
      cep: ['', Validators.required],
    }),
  });

  constructor() {
    if (this.ativoId !== null) {
      this.ativos.obterDetalhe(this.ativoId).subscribe({
        next: (detalhe) => {
          this.form.patchValue({
            apelido: detalhe.apelido,
            dataAquisicao: detalhe.dataAquisicao,
            valorAquisicao: detalhe.valorAquisicao,
            valorMercadoAtual: detalhe.valorMercadoAtual,
            tipoImovel: detalhe.imovel?.tipoImovel,
            areaM2: detalhe.imovel?.areaM2,
            matricula: detalhe.imovel?.matricula,
            valorIptuMensal: detalhe.imovel?.valorIptuMensal,
            valorCondominioMensal: detalhe.imovel?.valorCondominioMensal,
            endereco: detalhe.imovel
              ? { ...detalhe.imovel.endereco, complemento: detalhe.imovel.endereco.complemento ?? '' }
              : undefined,
          });
          if (detalhe.financiamento) {
            this.temFinanciamento.set(true);
            preencherFinanciamentoForm(this.financiamentoForm, detalhe.financiamento);
          }
          this.carregando.set(false);
        },
        error: () => {
          this.carregando.set(false);
          this.erro.set('Não foi possível carregar este Imóvel.');
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

    const request: ImovelRequest = {
      ...this.form.getRawValue(),
      financiamento: financiamentoFormParaDto(this.financiamentoForm, this.temFinanciamento()),
    };

    const requisicao =
      this.ativoId !== null
        ? this.ativos.atualizarImovel(this.ativoId, request)
        : this.ativos.criarImovel(request);

    requisicao.subscribe({
      next: (detalhe) => this.router.navigate(['/ativos', detalhe.id]),
      error: (erro: unknown) => {
        this.enviando.set(false);
        this.erro.set(mensagemErroAtivo(erro, 'Não foi possível salvar o Imóvel. Tente novamente.'));
      },
    });
  }
}
