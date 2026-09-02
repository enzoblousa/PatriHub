namespace PatriHub.Infrastructure.Identity;

/// <summary>
/// Decide se um JWT emitido em <paramref name="emitidoEm"/> ainda é válido pra
/// <paramref name="usuarioId"/>, comparando com <see cref="ApplicationUser.SenhaAlteradaEm"/> —
/// ver ADR-0009. Interface separada de <see cref="VerificadorSenhaAlterada"/> só pra dar um
/// seam de teste ao middleware da Api sem precisar de banco.
/// </summary>
public interface IVerificadorSenhaAlterada
{
    Task<bool> TokenAindaValidoAsync(Guid usuarioId, DateTimeOffset emitidoEm);
}
