import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';

import { Auth } from '../core/auth/auth';
import { mensagemErroHttp } from '../core/http/mensagem-erro-http';
import type { UsuarioDto } from '../core/auth/auth.models';

/**
 * Dados da própria conta (`GET /api/auth/me`, não o que ficou em `localStorage` desde o
 * login/registro) + exclusão de conta e dados (LGPD — ver ADR-0005). Confirmação inline antes
 * do `DELETE`, mesmo padrão de `AtivoDetalhe`/`LancamentosLista`/`ContratosLista` — exclusão
 * de conta é ainda mais irreversível (some com todo o histórico junto), mas o padrão de
 * confirmação é o mesmo já estabelecido no resto do app.
 */
@Component({
  selector: 'app-perfil',
  templateUrl: './perfil.html',
  styleUrl: './perfil.css',
})
export class Perfil {
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);

  protected readonly usuario = signal<UsuarioDto | null>(null);
  protected readonly carregando = signal(true);
  protected readonly erro = signal<string | null>(null);
  protected readonly confirmandoExclusao = signal(false);
  protected readonly excluindo = signal(false);

  constructor() {
    this.carregar();
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

    this.auth.excluirConta().subscribe({
      next: () => this.router.navigate(['/login'], { queryParams: { contaExcluida: 'true' } }),
      error: (erro: unknown) => {
        this.excluindo.set(false);
        this.confirmandoExclusao.set(false);
        this.erro.set(mensagemErroHttp(erro, 'Não foi possível excluir sua conta.'));
      },
    });
  }

  private carregar(): void {
    this.auth.me().subscribe({
      next: (usuario) => {
        this.usuario.set(usuario);
        this.carregando.set(false);
      },
      error: (erro: unknown) => {
        this.carregando.set(false);
        this.erro.set(mensagemErroHttp(erro, 'Não foi possível carregar os dados da conta.'));
      },
    });
  }
}
