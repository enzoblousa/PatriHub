import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { mensagemErroHttp } from '../../core/http/mensagem-erro-http';
import { Locatarios } from '../locatarios';
import type { LocatarioRequest } from '../locatarios.models';

/**
 * Cadastra e edita um Locatário na mesma tela: com `id` na rota (`/locatarios/:id/editar`) faz
 * PUT; sem `id` (`/locatarios/novo`) faz POST — mesmo padrão de `AtivoFormImovel`.
 */
@Component({
  selector: 'app-locatario-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './locatario-form.html',
  styleUrl: './locatario-form.css',
})
export class LocatarioForm {
  private readonly locatarios = inject(Locatarios);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly formBuilder = inject(FormBuilder);

  protected readonly locatarioId = this.route.snapshot.paramMap.get('id');
  protected readonly modoEdicao = this.locatarioId !== null;

  protected readonly carregando = signal(this.modoEdicao);
  protected readonly enviando = signal(false);
  protected readonly erro = signal<string | null>(null);

  protected readonly form = this.formBuilder.nonNullable.group({
    nome: ['', Validators.required],
    cpf: ['', Validators.required],
    telefone: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
  });

  constructor() {
    if (this.locatarioId !== null) {
      this.locatarios.obterDetalhe(this.locatarioId).subscribe({
        next: (locatario) => {
          this.form.patchValue(locatario);
          this.carregando.set(false);
        },
        error: () => {
          this.carregando.set(false);
          this.erro.set('Não foi possível carregar este Locatário.');
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

    const request: LocatarioRequest = this.form.getRawValue();

    const requisicao =
      this.locatarioId !== null
        ? this.locatarios.atualizar(this.locatarioId, request)
        : this.locatarios.criar(request);

    requisicao.subscribe({
      next: () => this.router.navigateByUrl('/locatarios'),
      error: (erro: unknown) => {
        this.enviando.set(false);
        this.erro.set(mensagemErroHttp(erro, 'Não foi possível salvar o Locatário. Tente novamente.'));
      },
    });
  }
}
