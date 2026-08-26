using PatriHub.Application.Ativos;
using PatriHub.Application.Common;
using PatriHub.Application.Lancamentos;

namespace PatriHub.Application.Admin;

/// <summary>
/// Ferramentas de suporte do Admin: gestão de contas de usuário e leitura auditada de
/// Ativos/Lançamentos de qualquer usuário (ver ADR-0002). Escrita nunca é exposta por aqui —
/// ativar/desativar e resetar senha mexem só na conta (Identity), nunca em Ativo/Lançamento de
/// outro usuário.
/// </summary>
public interface IAdminService
{
    Task<IReadOnlyList<UsuarioAdminDto>> ListarUsuariosAsync();

    Task<ResultadoOperacao> AtualizarStatusUsuarioAsync(Guid adminUsuarioId, Guid usuarioAlvoId, bool ativo);

    Task<ResultadoOperacao> ResetarSenhaAsync(Guid adminUsuarioId, Guid usuarioAlvoId, string novaSenha);

    Task<ResultadoOperacao<IReadOnlyList<AtivoResumoDto>>> ListarAtivosDoUsuarioAsync(Guid adminUsuarioId, Guid usuarioAlvoId);

    Task<ResultadoOperacao<AtivoDetalheDto>> ObterAtivoDoUsuarioAsync(Guid adminUsuarioId, Guid usuarioAlvoId, Guid ativoId);

    Task<ResultadoOperacao<IReadOnlyList<LancamentoDto>>> ListarLancamentosDoUsuarioAsync(Guid adminUsuarioId, Guid usuarioAlvoId, LancamentoFiltro filtro);

    Task<ResultadoOperacao<LancamentoDto>> ObterLancamentoDoUsuarioAsync(Guid adminUsuarioId, Guid usuarioAlvoId, Guid lancamentoId);
}
