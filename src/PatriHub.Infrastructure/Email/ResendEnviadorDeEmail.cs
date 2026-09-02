using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PatriHub.Infrastructure.Email;

/// <summary>
/// Envia via a API REST do Resend (POST /emails) direto com <see cref="HttpClient"/>, sem SDK —
/// é uma chamada só, não justifica mais uma dependência (mesmo racional da ADR-0007 evitando lib
/// de máscara). Registrado como cliente tipado em DependencyInjection; só é escolhido em runtime
/// quando `Resend:ApiKey` está configurada (ver <see cref="EnviadorDeEmailConsole"/> pro fallback
/// de dev). Remetente precisa ser um endereço em domínio verificado no Resend — ver ADR-0009.
/// </summary>
public sealed class ResendEnviadorDeEmail(HttpClient httpClient, IConfiguration configuration, ILogger<ResendEnviadorDeEmail> logger)
    : IEnviadorDeEmail
{
    public async Task EnviarRecuperacaoSenhaAsync(string destinatarioEmail, string destinatarioNome, string linkRecuperacao)
    {
        var remetenteEmail = configuration["Resend:RemetenteEmail"]
            ?? throw new InvalidOperationException("Resend:RemetenteEmail não configurado.");
        var remetenteNome = configuration["Resend:RemetenteNome"] ?? "PatriHub";

        var payload = new
        {
            from = $"{remetenteNome} <{remetenteEmail}>",
            to = new[] { destinatarioEmail },
            subject = "Recupere sua senha do PatriHub",
            html = MontarHtml(destinatarioNome, linkRecuperacao),
        };

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            configuration["Resend:ApiKey"]);

        var resposta = await httpClient.PostAsJsonAsync("emails", payload);
        if (!resposta.IsSuccessStatusCode)
        {
            // Não propaga a exceção pro chamador (AutenticacaoService trata como sucesso mesmo
            // assim — ver ADR-0009, evita vazar "o Resend está fora do ar" pro cliente e, mais
            // importante, não dá pra quem está testando emails alheios um sinal de que o envio
            // falhou por email não existir de verdade no Resend). Só fica registrado no log.
            var corpo = await resposta.Content.ReadAsStringAsync();
            logger.LogError(
                "Falha ao enviar email de recuperação de senha via Resend. Status: {Status}. Corpo: {Corpo}",
                resposta.StatusCode,
                corpo);
        }
    }

    private static string MontarHtml(string nome, string link) => $"""
        <p>Olá, {nome}.</p>
        <p>Recebemos um pedido para redefinir a senha da sua conta no PatriHub.</p>
        <p><a href="{link}">Clique aqui para criar uma nova senha</a>. Este link expira em 30 minutos.</p>
        <p>Se você não pediu essa redefinição, pode ignorar este email — sua senha continua a mesma.</p>
        """;
}
