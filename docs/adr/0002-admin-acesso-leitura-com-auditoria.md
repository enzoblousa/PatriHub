# Admin tem acesso de leitura a dados financeiros de qualquer usuário, com log de auditoria obrigatório

A proposta original do MVP era Admin nunca listar ativos/lançamentos de outros usuários
(consistente com o Princípio 3 da Constituição, "privacidade por padrão"). Essa decisão foi
revertida: Admin pode listar (somente leitura — nunca editar/excluir) ativos e lançamentos
de qualquer usuário, para viabilizar suporte direto sem precisar de acesso ao banco de
dados. Como envolve dado sensível sob LGPD (valores financeiros, CPF de locatário via
Contrato), todo acesso do Admin a dado de outro usuário é registrado em log de auditoria
(quem, quando, qual usuário/recurso) — sem isso não haveria como justificar o acesso numa
eventual auditoria. Vale para qualquer usuário com Role `Admin` no MVP, já que não existe
hierarquia de admin ainda; se o time de suporte crescer, reavaliar um nível mais restrito.

**Consequência**: a Constituição (Princípio 3) e a Spec Funcional (seção 7, regras de
autorização) foram atualizadas para refletir essa exceção — deixaram de dizer que o Admin
"não lista dados financeiros de outros usuários".
