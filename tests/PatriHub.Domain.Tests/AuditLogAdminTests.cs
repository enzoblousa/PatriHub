using PatriHub.Domain.Entidades;

namespace PatriHub.Domain.Tests;

public class AuditLogAdminTests
{
    [Fact]
    public void Registrar_com_admin_e_usuario_alvo_distintos_cria_log()
    {
        var adminId = Guid.NewGuid();
        var usuarioAlvoId = Guid.NewGuid();
        var recursoId = Guid.NewGuid();

        var log = AuditLogAdmin.Registrar(adminId, usuarioAlvoId, RecursoAuditoria.Ativos, recursoId);

        Assert.NotEqual(Guid.Empty, log.Id);
        Assert.Equal(adminId, log.AdminUsuarioId);
        Assert.Equal(usuarioAlvoId, log.UsuarioAlvoId);
        Assert.Equal(RecursoAuditoria.Ativos, log.Recurso);
        Assert.Equal(recursoId, log.RecursoId);
    }

    [Fact]
    public void Registrar_sem_recursoId_representa_acesso_a_uma_listagem()
    {
        var log = AuditLogAdmin.Registrar(Guid.NewGuid(), Guid.NewGuid(), RecursoAuditoria.Lancamentos);

        Assert.Null(log.RecursoId);
    }

    [Fact]
    public void Registrar_com_admin_igual_ao_usuario_alvo_lanca_ArgumentException()
    {
        var mesmoUsuario = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() => AuditLogAdmin.Registrar(mesmoUsuario, mesmoUsuario, RecursoAuditoria.Usuario));
    }
}
