import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { Auth } from '../../core/auth/auth';
import { mensagemErro } from '../../shared/formularios/mensagem-erro';
import { MENSAGENS_SENHA, senhaForteValidator } from '../senha-validadores';

@Component({
  selector: 'app-redefinir-senha',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './redefinir-senha.html',
  styleUrl: './redefinir-senha.css',
})
export class RedefinirSenha {
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);
  private readonly formBuilder = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);

  protected readonly enviando = signal(false);
  protected readonly erro = signal<string | null>(null);
  protected readonly mensagemErro = mensagemErro;
  protected readonly MENSAGENS_SENHA = MENSAGENS_SENHA;

  private readonly email = this.route.snapshot.queryParamMap.get('email') ?? '';
  private readonly token = this.route.snapshot.queryParamMap.get('token') ?? '';
  /** Sem email/token na URL o link já nasceu quebrado — nem mostra o formulário (ver ADR-0009). */
  protected readonly linkInvalido = !this.email || !this.token;

  protected readonly form = this.formBuilder.nonNullable.group({
    novaSenha: ['', [Validators.required, senhaForteValidator]],
  });

  protected redefinir(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.enviando.set(true);
    this.erro.set(null);

    this.auth
      .redefinirSenha({ email: this.email, token: this.token, novaSenha: this.form.getRawValue().novaSenha })
      .subscribe({
        // Sem login automático (Q11) — redireciona pro login, que mostra a confirmação via
        // ?senhaRedefinida=true (mesmo padrão de ?contaExcluida=true, ver Login).
        next: () => this.router.navigate(['/login'], { queryParams: { senhaRedefinida: 'true' } }),
        error: (erro: unknown) => {
          this.enviando.set(false);
          this.erro.set(
            erro instanceof HttpErrorResponse && typeof erro.error?.erro === 'string'
              ? erro.error.erro
              : 'Não foi possível redefinir a senha. Tente novamente.',
          );
        },
      });
  }
}
