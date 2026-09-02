import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { Auth } from '../../core/auth/auth';

@Component({
  selector: 'app-esqueci-senha',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './esqueci-senha.html',
  styleUrl: './esqueci-senha.css',
})
export class EsqueciSenha {
  private readonly auth = inject(Auth);
  private readonly formBuilder = inject(FormBuilder);

  protected readonly enviando = signal(false);
  protected readonly erro = signal<string | null>(null);
  /** Esconde o formulário depois do sucesso — não faz sentido deixar reenviar sem motivo (ver ADR-0009, Q10 já cobre limite no backend, isso aqui é só UX). */
  protected readonly enviado = signal(false);

  protected readonly form = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
  });

  protected enviar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.enviando.set(true);
    this.erro.set(null);

    this.auth.solicitarRecuperacaoSenha(this.form.getRawValue()).subscribe({
      next: () => {
        this.enviando.set(false);
        this.enviado.set(true);
      },
      error: (erro: unknown) => {
        this.enviando.set(false);
        this.erro.set(
          erro instanceof HttpErrorResponse && typeof erro.error?.erro === 'string'
            ? erro.error.erro
            : 'Não foi possível enviar o link de recuperação. Tente novamente.',
        );
      },
    });
  }
}
