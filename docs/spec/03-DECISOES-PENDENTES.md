# PatriHub — Decisões Pendentes

Lista consolidada de pontos deixados como "decidir depois" durante o levantamento inicial.
Cada item tem uma proposta de default para não travar o desenvolvimento — sinalize se
quiser mudar algum antes de eu avançar para a implementação.

| # | Tema | Pergunta em aberto | Default proposto |
|---|---|---|---|
| 1 | Papel Admin | Até onde o Admin pode ver dados financeiros de usuários, para fins de suporte? | Admin gerencia apenas contas (ativar/desativar/reset de senha); **não** lista ativos/lançamentos de outros usuários no MVP. |
| 2 | Contrato × Status do Ativo | Inadimplência é manual ou automática? Criar contrato muda o status do ativo para "Alugado" automaticamente? | Ambos manuais no MVP: usuário marca inadimplência e também atualiza o status do ativo. Automação fica para depois. |
| 3 | Categorias de despesa | Lista fechada pelo sistema ou usuário pode criar categorias próprias? | MVP com lista fixa pré-definida (`IPTU`, `Condomínio`, `Manutenção`, `Seguro`, `IPVA`, `Multa`, `Financiamento`, `Imposto de Renda`, `Outras`); categorias customizáveis ficam para uma v2. |
| 4 | Base do cálculo de ROI | ROI é sobre `ValorAquisicao` ou `ValorMercadoAtual`? | Calcular os dois e exibir lado a lado no dashboard (ROI sobre custo × ROI sobre valor atual), já que são leituras diferentes e úteis. |
| 5 | Taxa de referência (custo de oportunidade) | De onde vem a taxa (CDI/Selic)? Input manual ou integração futura? | Input manual por usuário no MVP (campo "taxa de referência anual %"); integração com índice oficial fica para depois. |
| 6 | Critério de projeção no dashboard | Como projetar lucro futuro? | Média simples dos últimos 3 meses de lucro, projetada linearmente — critério simples, revisável depois com o usuário validando se faz sentido. |
| 7 | Co-propriedade de ativos | Um ativo pode ter mais de um dono (ex.: casal)? | Fora do MVP — um ativo pertence a exatamente um usuário. Reavaliar em v2 junto com multiusuário por conta. |
| 8 | Identidade visual / referência de design | Nenhuma referência definida ainda. | Quando chegarmos na etapa de UI, uso uma referência neutra (dashboards financeiros tipo fintech) e valido com você antes de aplicar em todo o app. |

Assim que você validar (ou ajustar) esses pontos, sigo para o detalhamento técnico
(ex.: contratos de API, schema definitivo de migrations) e depois para a quebra em tarefas
de implementação.
