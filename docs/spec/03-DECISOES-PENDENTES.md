# PatriHub — Decisões (histórico)

Lista consolidada de pontos levantados como "decidir depois" durante o levantamento inicial.
Todas foram resolvidas em uma sessão de grilling — a decisão final de cada uma está em
[`../../CONTEXT.md`](../../CONTEXT.md) (glossário) ou em uma ADR (decisão arquitetural)
sob [`../adr/`](../adr/). Mantido aqui como registro histórico das alternativas consideradas.

| # | Tema | Pergunta original | Decisão final |
|---|---|---|---|
| 1 | Papel Admin | Até onde o Admin pode ver dados financeiros de usuários? | Leitura (nunca edição/exclusão) de qualquer usuário, com log de auditoria obrigatório. [ADR-0002](../adr/0002-admin-acesso-leitura-com-auditoria.md) |
| 2 | Contrato × Status do Ativo | Inadimplência é manual ou automática? Criar contrato muda o status automaticamente? | Ambos automáticos: contrato define `Alugado`/`Vago` no Ativo; inadimplência é detectada por job diário. [ADR-0003](../adr/0003-job-inadimplencia-background-service.md), [`CONTEXT.md`](../../CONTEXT.md) |
| 3 | Categorias de despesa | Lista fechada ou customizável? | Lista fixa no MVP: `IPTU, Condomínio, Manutenção, Reforma, Seguro, IPVA, Multa, Financiamento, Administração, Imposto de Renda, Outras`. Customização fica para v2. |
| 4 | Base do cálculo de ROI | `ValorAquisicao` ou `ValorMercadoAtual`? | As duas, exibidas lado a lado. Além disso, ROI e Yield foram separados em duas métricas distintas — ver [`CONTEXT.md`](../../CONTEXT.md). |
| 5 | Taxa de referência (custo de oportunidade) | Input manual ou integração? | Input manual (decorre do Princípio 1 da Constituição — manual antes de automático). |
| 6 | Critério de projeção no dashboard | Como projetar lucro futuro? | Média simples dos últimos 3 meses, projetada linearmente. |
| 7 | Co-propriedade de ativos | Um ativo pode ter mais de um dono? | Fora do MVP — já estava explícito na lista "Fora de escopo" da Constituição; não era, de fato, uma decisão em aberto. |
| 8 | Identidade visual / referência de design | Nenhuma referência definida ainda. | **Ainda em aberto, deliberadamente** — resolve quando chegarmos na etapa de UI. |

## Decisões adicionais que surgiram durante o grilling (não estavam na lista original)

| Tema | Decisão final |
|---|---|
| Yield vs. ROI | Eram tratados como sinônimo no glossário; viraram duas métricas distintas — ver [`CONTEXT.md`](../../CONTEXT.md). |
| Vínculo Lançamento ↔ Contrato | Adicionado `ContratoId` opcional/nullable em `Lançamento`, necessário para a detecção automática de inadimplência e para relatórios por locatário. |
| Refresh token | Sem refresh token no MVP; JWT de validade longa (ex.: 7 dias). [ADR-0001](../adr/0001-sem-refresh-token-mvp.md) |
