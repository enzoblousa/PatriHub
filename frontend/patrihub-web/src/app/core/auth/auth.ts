import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import type {
  LoginRequest,
  RegistrarUsuarioRequest,
  ResultadoAutenticacao,
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

  logout(): void {
    localStorage.removeItem(CHAVE_TOKEN);
    localStorage.removeItem(CHAVE_USUARIO);
    this.tokenSignal.set(null);
    this.usuarioSignal.set(null);
    void this.router.navigate(['/login']);
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
