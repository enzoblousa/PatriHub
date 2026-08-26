// Espelha os DTOs de PatriHub.Application.Admin (AdminDtos.cs) — camelCase, mesma convenção
// documentada em `../ativos/ativos.models.ts`. `AtivoResumoDto`/`LancamentoDto` são
// reaproveitados das respectivas features (mesmo shape retornado pelas rotas de admin — ver
// `AdminController.ListarAtivosDoUsuario`/`ListarLancamentosDoUsuario`).

/** Visão de conta usada pelo Admin pra localizar o usuário-alvo das ações de suporte. */
export interface UsuarioAdminDto {
  id: string;
  nome: string;
  email: string;
  papel: string;
  ativo: boolean;
  criadoEm: string;
}

export interface AtualizarStatusUsuarioRequest {
  ativo: boolean;
}

export interface ResetarSenhaRequest {
  novaSenha: string;
}
