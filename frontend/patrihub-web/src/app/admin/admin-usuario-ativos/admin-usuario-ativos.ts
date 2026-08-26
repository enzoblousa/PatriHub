import { DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { ROTULOS_STATUS, ROTULOS_TIPO } from '../../ativos/ativos-rotulos';
import type { AtivoResumoDto } from '../../ativos/ativos.models';
import { mensagemErroHttp } from '../../core/http/mensagem-erro-http';
import { Admin } from '../admin';

/**
 * Leitura auditada (ver ADR-0002), somente leitura — sem link de edição/exclusão: o Admin não
 * tem endpoint de escrita sobre Ativo de outro usuário (`AtivosController` continua filtrando
 * sempre pelo dono via token, ver `AdminController`).
 */
@Component({
  selector: 'app-admin-usuario-ativos',
  imports: [RouterLink, DecimalPipe],
  templateUrl: './admin-usuario-ativos.html',
  styleUrl: './admin-usuario-ativos.css',
})
export class AdminUsuarioAtivos {
  private readonly admin = inject(Admin);
  private readonly route = inject(ActivatedRoute);

  protected readonly usuarioId = this.route.snapshot.paramMap.get('id')!;
  protected readonly ativos = signal<AtivoResumoDto[]>([]);
  protected readonly carregando = signal(true);
  protected readonly erro = signal<string | null>(null);

  protected readonly rotulosTipo = ROTULOS_TIPO;
  protected readonly rotulosStatus = ROTULOS_STATUS;

  constructor() {
    this.admin.listarAtivosDoUsuario(this.usuarioId).subscribe({
      next: (ativos) => {
        this.ativos.set(ativos);
        this.carregando.set(false);
      },
      error: (erro: unknown) => {
        this.carregando.set(false);
        this.erro.set(mensagemErroHttp(erro, 'Não foi possível carregar os Ativos deste usuário.'));
      },
    });
  }
}
