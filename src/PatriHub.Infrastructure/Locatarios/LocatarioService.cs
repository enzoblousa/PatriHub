using Microsoft.EntityFrameworkCore;
using PatriHub.Application.Common;
using PatriHub.Application.Locatarios;
using PatriHub.Domain.Entidades;
using PatriHub.Infrastructure.Persistence;

namespace PatriHub.Infrastructure.Locatarios;

/// <summary>
/// Toda consulta filtra por `UsuarioId` diretamente na query (nunca checa dono depois de
/// carregar) — um Locatário de outro usuário simplesmente não aparece, o que satisfaz o
/// 404 de isolamento por conta (ver 01-SPEC-FUNCIONAL.md §7).
/// </summary>
public sealed class LocatarioService(PatriHubDbContext db) : ILocatarioService
{
    public async Task<ResultadoOperacao<LocatarioDto>> CriarAsync(Guid usuarioId, LocatarioRequest request)
    {
        Locatario? locatario = null;
        if (!TentarExecutar(() => locatario = Locatario.Cadastrar(usuarioId, request.Nome, request.Cpf, request.Telefone, request.Email), out var erro))
        {
            return erro!;
        }

        db.Locatarios.Add(locatario!);
        await db.SaveChangesAsync();
        return ResultadoOperacao<LocatarioDto>.ComSucesso(MapearDto(locatario!));
    }

    public async Task<ResultadoOperacao<LocatarioDto>> AtualizarAsync(Guid usuarioId, Guid locatarioId, LocatarioRequest request)
    {
        var locatario = await BuscarLocatarioDoUsuarioAsync(usuarioId, locatarioId);
        if (locatario is null)
        {
            return ResultadoOperacao<LocatarioDto>.ComErro("Locatário não encontrado.", TipoErroOperacao.NaoEncontrado);
        }

        if (!TentarExecutar(() => locatario.Atualizar(request.Nome, request.Cpf, request.Telefone, request.Email), out var erro))
        {
            return erro!;
        }

        await db.SaveChangesAsync();
        return ResultadoOperacao<LocatarioDto>.ComSucesso(MapearDto(locatario));
    }

    public async Task<ResultadoOperacao<LocatarioDto>> ObterDetalheAsync(Guid usuarioId, Guid locatarioId)
    {
        var locatario = await BuscarLocatarioDoUsuarioAsync(usuarioId, locatarioId);
        if (locatario is null)
        {
            return ResultadoOperacao<LocatarioDto>.ComErro("Locatário não encontrado.", TipoErroOperacao.NaoEncontrado);
        }

        return ResultadoOperacao<LocatarioDto>.ComSucesso(MapearDto(locatario));
    }

    public async Task<IReadOnlyList<LocatarioDto>> ListarAsync(Guid usuarioId)
    {
        var locatarios = await db.Locatarios
            .Where(l => l.UsuarioId == usuarioId)
            .OrderBy(l => l.Nome)
            .ToListAsync();

        return locatarios.Select(MapearDto).ToList();
    }

    private Task<Locatario?> BuscarLocatarioDoUsuarioAsync(Guid usuarioId, Guid locatarioId) =>
        db.Locatarios.FirstOrDefaultAsync(l => l.Id == locatarioId && l.UsuarioId == usuarioId);

    /// <summary>
    /// Roda um cadastro/edição de domínio (que valida e pode lançar <see cref="ArgumentException"/>)
    /// e converte a exceção num <see cref="TipoErroOperacao.Validacao"/> — usado por todo método
    /// que constrói ou atualiza um Locatário, para não repetir o mesmo try/catch em cada um.
    /// </summary>
    private static bool TentarExecutar(Action acao, out ResultadoOperacao<LocatarioDto>? erro)
    {
        try
        {
            acao();
            erro = null;
            return true;
        }
        catch (ArgumentException ex)
        {
            erro = ResultadoOperacao<LocatarioDto>.ComErro(ex.Message, TipoErroOperacao.Validacao);
            return false;
        }
    }

    private static LocatarioDto MapearDto(Locatario locatario) => new(
        locatario.Id,
        locatario.Nome,
        locatario.Cpf,
        locatario.Telefone,
        locatario.Email,
        locatario.CriadoEm,
        locatario.AtualizadoEm);
}
