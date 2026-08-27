// Espelha os DTOs de PatriHub.Application.Autenticacao (AutenticacaoDtos.cs). O
// System.Text.Json do backend serializa em camelCase por padrão — os nomes de campo aqui
// batem com o JSON retornado, não com o nome C# original.

export interface RegistrarUsuarioRequest {
  nome: string;
  email: string;
  senha: string;
  consentimentoLgpd: boolean;
}

export interface LoginRequest {
  email: string;
  senha: string;
}

export interface UsuarioDto {
  id: string;
  nome: string;
  email: string;
  papel: string;
}

export interface ResultadoAutenticacao {
  sucesso: boolean;
  erro: string | null;
  token: string | null;
  expiraEm: string | null;
  usuario: UsuarioDto | null;
}
