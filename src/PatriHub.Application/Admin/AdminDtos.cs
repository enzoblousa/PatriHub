namespace PatriHub.Application.Admin;

/// <summary>Visão de conta usada pelo Admin para localizar o usuário-alvo das ações de suporte (ver AC "gerenciar contas").</summary>
public sealed record UsuarioAdminDto(Guid Id, string Nome, string Email, string Papel, bool Ativo, DateTimeOffset CriadoEm);

public sealed record AtualizarStatusUsuarioRequest(bool Ativo);

public sealed record ResetarSenhaRequest(string NovaSenha);
