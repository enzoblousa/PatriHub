using Microsoft.Extensions.Logging;

namespace PatriHub.Infrastructure.Email;

/// <summary>
/// Fallback de desenvolvimento: em vez de mandar email de verdade, só loga o link — assim dá
/// pra testar o fluxo de "esqueci minha senha" localmente sem precisar de uma conta no Resend
/// nem de domínio verificado. Escolhido em DependencyInjection só quando `Resend:ApiKey` está
/// vazio/ausente (nunca em produção, onde a env var é obrigatória — ver render.yaml).
/// </summary>
public sealed class EnviadorDeEmailConsole(ILogger<EnviadorDeEmailConsole> logger) : IEnviadorDeEmail
{
    public Task EnviarRecuperacaoSenhaAsync(string destinatarioEmail, string destinatarioNome, string linkRecuperacao)
    {
        logger.LogWarning(
            "Resend:ApiKey não configurada — email de recuperação de senha NÃO foi enviado de verdade. " +
            "Destinatário: {Email}. Link: {Link}",
            destinatarioEmail,
            linkRecuperacao);

        return Task.CompletedTask;
    }
}
