# Detecção de inadimplência via job periódico in-process, não fila/agendador dedicado

Um Contrato `Ativo` vira `Inadimplente` quando passam 5 dias de carência após o vencimento
sem um Lançamento (Receita, categoria Aluguel, mesmo `ContratoId`) dentro do mês de
competência. Essa checagem roda uma vez por dia via um `BackgroundService` (`IHostedService`)
dentro do próprio processo da API .NET, em vez de uma ferramenta dedicada de agendamento
(Hangfire, Quartz.NET, Azure Function com Timer Trigger). O Plano Técnico até então dizia
"sem mensageria/fila necessária no MVP" — esse job é a primeira exceção, mas optamos pela
opção mais simples (sem dependência nova, sem tabela de controle de jobs) porque o MVP roda
como instância única no Azure App Service e a carência de 5 dias já absorve qualquer atraso
de execução. Se o produto crescer para múltiplas instâncias ou precisar de jobs mais
complexos/reentrantes, migrar para Hangfire/Quartz nesse ponto.
