namespace PatriHub.Domain.Entidades;

/// <summary>Recurso do usuário-alvo acessado pelo Admin — ver 02-PLANO-TECNICO.md §3.</summary>
public enum RecursoAuditoria
{
    Usuario,
    Ativos,
    Lancamentos
}

/// <summary>
/// Registro de um acesso do Admin a dado de outro usuário (ver ADR-0002, CONTEXT.md "Admin").
/// `RecursoId` é nulo quando o acesso é a uma listagem (ex.: todos os Ativos de um usuário) e
/// preenchido quando é a um registro específico. Nunca gravado para o Admin acessar os
/// próprios dados — a auditoria existe só para dado de outro usuário.
/// </summary>
public sealed class AuditLogAdmin
{
    public Guid Id { get; private set; }
    public Guid AdminUsuarioId { get; private set; }
    public Guid UsuarioAlvoId { get; private set; }
    public RecursoAuditoria Recurso { get; private set; }
    public Guid? RecursoId { get; private set; }
    public DateTimeOffset CriadoEm { get; private set; }

    private AuditLogAdmin()
    {
        // EF Core
    }

    private AuditLogAdmin(Guid adminUsuarioId, Guid usuarioAlvoId, RecursoAuditoria recurso, Guid? recursoId, DateTimeOffset criadoEm)
    {
        Id = Guid.NewGuid();
        AdminUsuarioId = adminUsuarioId;
        UsuarioAlvoId = usuarioAlvoId;
        Recurso = recurso;
        RecursoId = recursoId;
        CriadoEm = criadoEm;
    }

    public static AuditLogAdmin Registrar(Guid adminUsuarioId, Guid usuarioAlvoId, RecursoAuditoria recurso, Guid? recursoId = null, DateTimeOffset? agora = null)
    {
        if (adminUsuarioId == usuarioAlvoId)
        {
            throw new ArgumentException(
                "Acesso do Admin aos próprios dados não gera log de auditoria — ver ADR-0002.",
                nameof(usuarioAlvoId));
        }

        return new AuditLogAdmin(adminUsuarioId, usuarioAlvoId, recurso, recursoId, agora ?? DateTimeOffset.UtcNow);
    }
}
