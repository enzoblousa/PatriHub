import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { Ativos } from '../../ativos/ativos';
import { mensagemErroHttp } from '../../core/http/mensagem-erro-http';
import { Contratos } from '../contratos';
import type { ContratoRequest } from '../contratos.models';
import { Locatarios } from '../locatarios';

/**
 * Cria um Contrato vinculando um Ativo a um Locatário — sem edição no MVP (ver
 * `ContratoRequest`), só criação e encerramento (`ContratosLista`). O Ativo pode vir
 * pré-selecionado por `?ativoId=` (link "Novo Contrato" na tela de detalhe do Ativo), mesmo
 * padrão de `LancamentoForm`.
 */
@Component({
  selector: 'app-contrato-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './contrato-form.html',
  styleUrl: './contrato-form.css',
})
export class ContratoForm {
  private readonly contratos = inject(Contratos);
  protected readonly ativos = inject(Ativos);
  protected readonly locatarios = inject(Locatarios);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly formBuilder = inject(FormBuilder);

  protected readonly enviando = signal(false);
  protected readonly erro = signal<string | null>(null);

  protected readonly form = this.formBuilder.nonNullable.group({
    ativoId: [this.route.snapshot.queryParamMap.get('ativoId') ?? '', Validators.required],
    locatarioId: ['', Validators.required],
    valorAluguelMensal: [0, [Validators.required, Validators.min(0.01)]],
    diaVencimento: [10, [Validators.required, Validators.min(1), Validators.max(31)]],
    dataInicio: ['', Validators.required],
    dataFim: [''],
  });

  constructor() {
    this.ativos.carregarLista();
    this.locatarios.carregarLista();
  }

  protected salvar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.enviando.set(true);
    this.erro.set(null);

    const valores = this.form.getRawValue();
    const request: ContratoRequest = {
      ...valores,
      dataFim: valores.dataFim.trim() === '' ? null : valores.dataFim,
    };

    this.contratos.criar(request).subscribe({
      next: () => this.router.navigateByUrl('/contratos'),
      error: (erro: unknown) => {
        this.enviando.set(false);
        this.erro.set(mensagemErroHttp(erro, 'Não foi possível criar o Contrato. Tente novamente.'));
      },
    });
  }
}
