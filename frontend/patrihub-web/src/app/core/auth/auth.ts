import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import type {
  LoginRequest,
  RedefinirSenhaRequest,
  RegistrarUsuarioRequest,
  ResultadoAutenticacao,
  SolicitarRecuperacaoSenhaRequest,
  UsuarioDto,
} from './auth.models';

const CHAVE_TOKEN = 'patrihub.token';
const CHAVE_USUARIO = 'patrihub.usuario';

/**
 * Sessão do usuário logado. Token JWT guardado em `localStorage` (ver ADR-0004) — nunca em
 * cookie httpOnly. `estaAutenticado`/`usuario` são Signals lidos por guards, pelo interceptor
 * e pelo shell do app; nenhum outro lugar do código deve ler `localStorage` diretamente.
 */
@Injectable({
  providedIn: 'root',
})
export class Auth {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly tokenSignal = signal<string | null>(localStorage.getItem(CHAVE_TOKEN));
  private readonly usuarioSignal = signal<UsuarioDto | null>(lerUsuarioArmazenado());

  readonly usuario = this.usuarioSignal.asReadonly();
  readonly estaAutenticado = computed(() => this.tokenSignal() !== null);

  obterToken(): string | null {
    return this.tokenSignal();
  }

  registrar(request: RegistrarUsuarioRequest) {
    return this.http
      .post<ResultadoAutenticacao>(`${environment.apiBaseUrl}/api/auth/registrar`, request)
      .pipe(tap((resultado) => this.persistirSessao(resultado)));
  }

  login(request: LoginRequest) {
    return this.http
      .post<ResultadoAutenticacao>(`${environment.apiBaseUrl}/api/auth/login`, request)
      .pipe(tap((resultado) => this.persistirSessao(resultado)));
  }

  /**
   * "Esqueci minha senha" (ver ADR-0009). Sem `tap`/persistência de sessão — esse passo não
   * autentica ninguém, só dispara o email. 404 (email não encontrado) e 400 chegam como erro
   * pro `subscribe` de quem chama, mesmo padrão de `login`/`registrar`.
   */
  solicitarRecuperacaoSenha(request: SolicitarRecuperacaoSenhaRequest) {
    return this.http.post<void>(`${environment.apiBaseUrl}/api/auth/esqueci-senha`, request);
  }

  /**
   * Conclui a recuperação de senha a partir do token do link do email (ver ADR-0009). Também
   * sem `tap`: login automático pós-reset foi decidido fora de escopo (Q11) — quem chama
   * redireciona pro `/login` manualmente depois do sucesso.
   */
  redefinirSenha(request: RedefinirSenhaRequest) {
    return this.http.post<void>(`${environment.apiBaseUrl}/api/auth/redefinir-senha`, request);
  }

  /** Dados da própria conta a partir das claims do token (`GET /api/auth/me`) — usado pelo Perfil pra nunca depender só do que ficou em `localStorage` desde o login/registro. */
  me() {
    return this.http.get<UsuarioDto>(`${environment.apiBaseUrl}/api/auth/me`);
  }

  /** Exclusão definitiva da própria conta e dados (LGPD — ver ADR-0005). Só limpa a sessão local; navegar pro login com a mensagem de confirmação é responsabilidade de quem chama (ver Perfil). */
  excluirConta() {
    return this.http
      .delete<void>(`${environment.apiBaseUrl}/api/auth/conta`)
      .pipe(tap(() => this.limparSessao()));
  }

  logout(): void {
    this.limparSessao();
    void this.router.navigate(['/login']);
  }

  private limparSessao(): void {
    localStorage.removeItem(CHAVE_TOKEN);
    localStorage.removeItem(CHAVE_USUARIO);
    this.tokenSignal.set(null);
    this.usuarioSignal.set(null);
  }

  private persistirSessao(resultado: ResultadoAutenticacao): void {
    if (!resultado.sucesso || !resultado.token || !resultado.usuario) {
      return;
    }

    localStorage.setItem(CHAVE_TOKEN, resultado.token);
    localStorage.setItem(CHAVE_USUARIO, JSON.stringify(resultado.usuario));
    this.tokenSignal.set(resultado.token);
    this.usuarioSignal.set(resultado.usuario);
  }
}

function lerUsuarioArmazenado(): UsuarioDto | null {
  const bruto = localStorage.getItem(CHAVE_USUARIO);
  if (!bruto) {
    return null;
  }

  try {
    return JSON.parse(bruto) as UsuarioDto;
  } catch {
    return null;
  }
}
