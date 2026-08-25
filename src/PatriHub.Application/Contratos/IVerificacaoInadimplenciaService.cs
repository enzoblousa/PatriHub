namespace PatriHub.Application.Contratos;

/// <summary>
/// Checagem de inadimplência (ADR-0003): um <c>BackgroundService</c> dispara
/// <see cref="VerificarAsync"/> uma vez por dia, mas o método em si não depende do timer — é
/// chamável diretamente em teste. Varre todos os usuários (é um job de sistema, não uma operação
/// de um usuário autenticado).
/// </summary>
public interface IVerificacaoInadimplenciaService
{
    /// <summary>
    /// Marca como `Inadimplente` todo Contrato `Ativo` cuja carência de 5 dias após o vencimento
    /// do mês de <paramref name="hoje"/> já passou sem um Lançamento (Receita, categoria Aluguel,
    /// mesmo `ContratoId`) dentro desse mês de competência.
    /// </summary>
    /// <param name="hoje">Data de referência; usada em teste para não depender do relógio real.</param>
    Task VerificarAsync(DateOnly? hoje = null);
}
