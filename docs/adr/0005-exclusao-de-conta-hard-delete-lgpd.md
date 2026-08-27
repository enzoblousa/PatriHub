# Exclusão de conta (LGPD) é hard delete em cascata, sem anonimização

O §8 da Spec Funcional exige que o usuário possa solicitar a exclusão da própria conta e
dados, mas nunca detalhou o mecanismo. Duas opções: hard delete (remove as linhas de
verdade) ou soft delete/anonimização (mantém o histórico financeiro anonimizado, só some o
dado pessoal). Optou-se por hard delete completo — a conta (`ApplicationUser`) e todo o
histórico do usuário (Ativos — incluindo `Imoveis`/`Carros` via TPT —, Lançamentos,
Contratos, Locatários) são removidos de verdade, sem anonimização.

Motivos: (1) não existe hoje nenhuma funcionalidade de analytics agregado entre usuários
(dashboard é sempre por usuário — ver `01-SPEC-FUNCIONAL.md` §6.4) que se beneficiaria de
manter histórico anonimizado; (2) anonimização exigiria decidir uma estratégia própria por
entidade (o que anonimizar em Locatário — CPF de terceiro — vs. em Lançamento) sem
nenhum requisito concreto puxando essa complexidade agora, o que fere o Princípio 2 da
Constituição ("simples antes de completo"); (3) hard delete é o que o texto de confirmação
já promete ao usuário ("serão permanentemente apagados").

Ordem de exclusão segue as FKs `Restrict` já existentes no modelo (`PatriHubDbContext`):
Lançamentos antes de Contratos e Ativos, Contratos antes de Ativos e Locatários — tudo numa
única transação com a remoção do `ApplicationUser` via `UserManager.DeleteAsync`, para nunca
sobrar histórico órfão se a exclusão do usuário no Identity falhar (ver
`AutenticacaoService.ExcluirContaAsync`).

**Ficam de fora do hard delete**: linhas de `AuditLogsAdmin` onde o usuário excluído é o
`UsuarioAlvoId` — o log existe para responsabilizar a conduta do Admin (ver ADR-0002), não
para rastrear o usuário-alvo, e sobrevive à conta que documentou.

**Limitação aceita, consistente com ADR-0001 (sem refresh token/revogação)**: o JWT emitido
antes da exclusão continua criptograficamente válido até expirar (~7 dias) mesmo depois da
conta ser removida — chamadas subsequentes com esse token a endpoints que filtram por
`UsuarioId` (ex.: `GET /api/ativos`) simplesmente não encontram mais nada, já que os dados
sumiram; nenhuma verificação adicional de "usuário ainda existe" acontece por request. Se
isso se tornar um problema real, é o mesmo gap que motivaria revisitar ADR-0001.
