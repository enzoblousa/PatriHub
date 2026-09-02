namespace PatriHub.Infrastructure.Email;

/// <summary>
/// Envio de email transacional — hoje só a recuperação de senha (ver ADR-0009). Sem
/// abstração de "template genérico": cada tipo de email tem seu próprio método, porque o
/// conteúdo (e a decisão de quando reenviar) é específico o bastante pra não valer a pena
/// generalizar ainda. Ver <see cref="ResendEnviadorDeEmail"/> (produção) e
/// <see cref="EnviadorDeEmailConsole"/> (dev, quando `Resend:ApiKey` não está configurada).
/// </summary>
public interface IEnviadorDeEmail
{
    Task EnviarRecuperacaoSenhaAsync(string destinatarioEmail, string destinatarioNome, string linkRecuperacao);
}
