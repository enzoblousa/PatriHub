using Microsoft.EntityFrameworkCore;
using PatriHub.Application.Common;
using PatriHub.Application.Contratos;
using PatriHub.Domain.Entidades;
using PatriHub.Infrastructure.Persistence;

namespace PatriHub.Infrastructure.Contratos;

/// <summary>
/// Toda consulta filtra por `UsuarioId` diretamente na query (nunca checa dono depois de
/// carregar) — um Contrato de outro usuário simplesmente não aparece, o que satisfaz o
/// 404 de isolamento por conta (ver 01-SPEC-FUNCIONAL.md §7). Criar/encerrar sincronizam o
/// Status do Ativo na mesma transação (mesmo SaveChangesAsync) — ver
/// <see cref="Ativo.MarcarAlugado"/>/<see cref="Ativo.MarcarVago"/>.
/// </summary>
public sealed class ContratoService(PatriHubDbContext db) : IContratoService
{
    public async Task<ResultadoOperacao<ContratoDto>> CriarAsync(Guid usuarioId, ContratoRequest request)
    {
        var ativo = await BuscarAtivoDoUsuarioAsync(usuarioId, request.AtivoId);
        if (ativo is null)
        {
            return ResultadoOperacao<ContratoDto>.ComErro("Ativo não encontrado.", TipoErroOperacao.NaoEncontrado);
        }

        if (!await LocatarioPertenceAoUsuarioAsync(usuarioId, request.LocatarioId))
        {
            return ResultadoOperacao<ContratoDto>.ComErro("Locatário não encontrado.", TipoErroOperacao.NaoEncontrado);
        }

        var ativoJaTemContratoAtivo = await db.Contratos
            .AnyAsync(c => c.AtivoId == request.AtivoId && c.Status == StatusContrato.Ativo);
        if (ativoJaTemContratoAtivo)
        {
            return ResultadoOperacao<ContratoDto>.ComErro("Este Ativo já possui um Contrato Ativo.", TipoErroOperacao.Validacao);
        }

        Contrato? contrato = null;
        if (!TentarExecutar(() => contrato = Contrato.Cadastrar(
                usuarioId,
                request.AtivoId,
                request.LocatarioId,
                request.ValorAluguelMensal,
                request.DiaVencimento,
                request.DataInicio,
                request.DataFim),
            out var erro))
        {
            return erro!;
        }

        // Sincronização semi-automática (01-SPEC-FUNCIONAL.md §6.3): sempre sobrepõe um status
        // manual anterior (Manutenção/À venda), que só prevalece até o próximo evento de contrato.
        ativo.MarcarAlugado();
        db.Contratos.Add(contrato!);
        await db.SaveChangesAsync();
        return ResultadoOperacao<ContratoDto>.ComSucesso(MapearDto(contrato!));
    }

    public async Task<ResultadoOperacao<ContratoDto>> EncerrarAsync(Guid usuarioId, Guid contratoId)
    {
        var contrato = await BuscarContratoDoUsuarioAsync(usuarioId, contratoId);
        if (contrato is null)
        {
            return ResultadoOperacao<ContratoDto>.ComErro("Contrato não encontrado.", TipoErroOperacao.NaoEncontrado);
        }

        if (!TentarExecutar(() => contrato.Encerrar(), out var erro))
        {
            return erro!;
        }

        var ativo = await db.Ativos.FirstOrDefaultAsync(a => a.Id == contrato.AtivoId);
        ativo?.MarcarVago();

        await db.SaveChangesAsync();
        return ResultadoOperacao<ContratoDto>.ComSucesso(MapearDto(contrato));
    }

    public async Task<ResultadoOperacao<ContratoDto>> ObterDetalheAsync(Guid usuarioId, Guid contratoId)
    {
        var contrato = await BuscarContratoDoUsuarioAsync(usuarioId, contratoId);
        if (contrato is null)
        {
            return ResultadoOperacao<ContratoDto>.ComErro("Contrato não encontrado.", TipoErroOperacao.NaoEncontrado);
        }

        return ResultadoOperacao<ContratoDto>.ComSucesso(MapearDto(contrato));
    }

    public async Task<IReadOnlyList<ContratoDto>> ListarAsync(Guid usuarioId)
    {
        var contratos = await db.Contratos
            .Where(c => c.UsuarioId == usuarioId)
            .OrderByDescending(c => c.CriadoEm)
            .ToListAsync();

        return contratos.Select(MapearDto).ToList();
    }

    private Task<Ativo?> BuscarAtivoDoUsuarioAsync(Guid usuarioId, Guid ativoId) =>
        db.Ativos.FirstOrDefaultAsync(a => a.Id == ativoId && a.UsuarioId == usuarioId && a.ExcluidoEm == null);

    private Task<bool> LocatarioPertenceAoUsuarioAsync(Guid usuarioId, Guid locatarioId) =>
        db.Locatarios.AnyAsync(l => l.Id == locatarioId && l.UsuarioId == usuarioId);

    private Task<Contrato?> BuscarContratoDoUsuarioAsync(Guid usuarioId, Guid contratoId) =>
        db.Contratos.FirstOrDefaultAsync(c => c.Id == contratoId && c.UsuarioId == usuarioId);

    /// <summary>
    /// Roda uma criação/edição de domínio (que valida e pode lançar <see cref="ArgumentException"/>)
    /// e converte a exceção num <see cref="TipoErroOperacao.Validacao"/> — usado por todo método
    /// que constrói ou atualiza um Contrato, para não repetir o mesmo try/catch em cada um.
    /// </summary>
    private static bool TentarExecutar(Action acao, out ResultadoOperacao<ContratoDto>? erro)
    {
        try
        {
            acao();
            erro = null;
            return true;
        }
        catch (ArgumentException ex)
        {
            erro = ResultadoOperacao<ContratoDto>.ComErro(ex.Message, TipoErroOperacao.Validacao);
            return false;
        }
    }

    private static ContratoDto MapearDto(Contrato contrato) => new(
        contrato.Id,
        contrato.AtivoId,
        contrato.LocatarioId,
        contrato.ValorAluguelMensal,
        contrato.DiaVencimento,
        contrato.DataInicio,
        contrato.DataFim,
        contrato.Status,
        contrato.CriadoEm,
        contrato.AtualizadoEm);
}
