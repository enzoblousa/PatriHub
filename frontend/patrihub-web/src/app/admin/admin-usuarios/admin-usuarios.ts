import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { Auth } from '../../core/auth/auth';
import { mensagemErroHttp } from '../../core/http/mensagem-erro-http';
import { Admin } from '../admin';
import type { UsuarioAdminDto } from '../admin.models';

/**
 * Gestão de contas: lista via `GET /api/admin/usuarios`, ativa/desativa via
 * `PATCH .../status` e reseta senha via `POST .../resetar-senha` — ambas as ações mudam
 * dado sensível (bloqueiam login / trocam credencial), então pedem confirmação/preenchimento
 * inline antes de chamar a API, mesmo espírito de `AtivoDetalhe`/`ContratosLista`.
 */
@Component({
  selector: 'app-admin-usuarios',
  imports: [RouterLink, ReactiveFormsModule],
  templateUrl: './admin-usuarios.html',
  styleUrl: './admin-usuarios.css',
})
export class AdminUsuarios {
  protected readonly admin = inject(Admin);
  protected readonly auth = inject(Auth);
  private readonly formBuilder = inject(FormBuilder);

  protected readonly erro = signal<string | null>(null);
  protected readonly atualizandoStatusId = signal<string | null>(null);
  protected readonly confirmandoStatusId = signal<string | null>(null);
  protected readonly resetandoSenhaId = signal<string | null>(null);
  protected readonly enviandoResetId = signal<string | null>(null);

  protected readonly resetSenhaForm = this.formBuilder.nonNullable.group({
    novaSenha: ['', Validators.required],
  });

  constructor() {
    this.admin.carregarUsuarios();
  }

  /** O próprio Admin não pode mudar o status da própria conta (backend rejeita com 400). */
  protected ehAPropriaConta(usuario: UsuarioAdminDto): boolean {
    return usuario.id === this.auth.usuario()?.id;
  }

  protected iniciarAlteracaoStatus(id: string): void {
    this.confirmandoStatusId.set(id);
  }

  protected cancelarAlteracaoStatus(): void {
    this.confirmandoStatusId.set(null);
  }

  protected confirmarAlteracaoStatus(usuario: UsuarioAdminDto): void {
    this.atualizandoStatusId.set(usuario.id);
    this.erro.set(null);

    this.admin.atualizarStatus(usuario.id, !usuario.ativo).subscribe({
      next: () => {
        this.atualizandoStatusId.set(null);
        this.confirmandoStatusId.set(null);
        this.admin.carregarUsuarios();
      },
      error: (erro: unknown) => {
        this.atualizandoStatusId.set(null);
        this.confirmandoStatusId.set(null);
        this.erro.set(mensagemErroHttp(erro, 'Não foi possível atualizar o status desta conta.'));
      },
    });
  }

  protected iniciarResetSenha(id: string): void {
    this.resetSenhaForm.reset();
    this.resetandoSenhaId.set(id);
  }

  protected cancelarResetSenha(): void {
    this.resetandoSenhaId.set(null);
  }

  protected confirmarResetSenha(usuarioId: string): void {
    if (this.resetSenhaForm.invalid) {
      this.resetSenhaForm.markAllAsTouched();
      return;
    }

    this.enviandoResetId.set(usuarioId);
    this.erro.set(null);

    this.admin.resetarSenha(usuarioId, this.resetSenhaForm.getRawValue().novaSenha).subscribe({
      next: () => {
        this.enviandoResetId.set(null);
        this.resetandoSenhaId.set(null);
      },
      error: (erro: unknown) => {
        this.enviandoResetId.set(null);
        this.erro.set(mensagemErroHttp(erro, 'Não foi possível resetar a senha deste usuário.'));
      },
    });
  }
}
