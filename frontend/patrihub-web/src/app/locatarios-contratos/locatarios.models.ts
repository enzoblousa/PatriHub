// Espelha os DTOs de PatriHub.Application.Locatarios (LocatarioDtos.cs) — camelCase, mesma
// convenção documentada em `../ativos/ativos.models.ts`. Locatário não tem enum, então não há
// convenção de enum numérico aqui.

/** Corpo de cadastro (POST) e edição (PUT) de um Locatário — os mesmos campos são exigidos nos dois casos. */
export interface LocatarioRequest {
  nome: string;
  cpf: string;
  telefone: string;
  email: string;
}

export interface LocatarioDto {
  id: string;
  nome: string;
  cpf: string;
  telefone: string;
  email: string;
  criadoEm: string;
  atualizadoEm: string;
}
