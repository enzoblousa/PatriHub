import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { catchError, of } from 'rxjs';

import { environment } from '../../environments/environment';
import type { AtivoResumoDto } from '../ativos/ativos.models';
import type { LancamentoDto } from '../lancamentos/lancamentos.models';
import type { UsuarioAdminDto } from './admin.models';

/**
 * Ferramentas de suporte do Admin (ver ADR-0002): gestão de contas e leitura auditada de
 * Ativos/Lançamentos de qualquer usuário. Sem escrita sobre dado de outro usuário — só a
 * conta (ativar/desativar, resetar senha) é mutável por aqui, espelhando `IAdminService`.
 */
@Injectable({
  providedIn: 'root',
})
export class Admin {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/admin`;

  private readonly usuariosSignal = signal<UsuarioAdminDto[]>([]);
  private readonly carregandoSignal = signal(false);

  readonly usuarios = this.usuariosSignal.asReadonly();
  readonly carregando = this.carregandoSignal.asReadonly();

  carregarUsuarios(): void {
    this.carregandoSignal.set(true);

    this.http
      .get<UsuarioAdminDto[]>(`${this.baseUrl}/usuarios`)
      .pipe(
        catchError(() => {
          this.carregandoSignal.set(false);
          return of(null);
        }),
      )
      .subscribe((usuarios) => {
        if (usuarios !== null) {
          this.usuariosSignal.set(usuarios);
          this.carregandoSignal.set(false);
        }
      });
  }

  atualizarStatus(usuarioId: string, ativo: boolean) {
    return this.http.patch<void>(`${this.baseUrl}/usuarios/${usuarioId}/status`, { ativo });
  }

  resetarSenha(usuarioId: string, novaSenha: string) {
    return this.http.post<void>(`${this.baseUrl}/usuarios/${usuarioId}/resetar-senha`, {
      novaSenha,
    });
  }

  listarAtivosDoUsuario(usuarioId: string) {
    return this.http.get<AtivoResumoDto[]>(`${this.baseUrl}/usuarios/${usuarioId}/ativos`);
  }

  listarLancamentosDoUsuario(usuarioId: string) {
    return this.http.get<LancamentoDto[]>(`${this.baseUrl}/usuarios/${usuarioId}/lancamentos`);
  }
}
