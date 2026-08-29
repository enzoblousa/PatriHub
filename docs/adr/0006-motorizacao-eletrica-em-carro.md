# Carro elétrico: um campo `Motorização` com unidade condicional, não campos separados

Ao adicionar suporte a Carro elétrico, decidimos reaproveitar o campo `ConsumoMedio`
existente em vez de criar `ConsumoMedioKmPorLitro`/`ConsumoMedioKmPorKwh` separados: um novo
enum `Motorização` (`Combustão` | `Elétrico`) determina só a unidade de leitura do mesmo
número. Optamos pelo campo único porque o escopo foi deliberadamente restrito a 100%
elétrico (BEV) — híbridos (HEV/PHEV), que teriam consumo misto e justificariam dois campos,
ficam de fora por ora e são classificados como `Combustão`. Se o suporte a híbridos entrar
depois, essa decisão deve ser revisitada.

Na mesma leva, unificamos a categoria de despesa de reabastecimento/recarga em uma única
`Abastecimento` em vez de `Combustível` + `Recarga` separadas — um Carro só tem uma
`Motorização` por vez, então não há ambiguidade em qual categoria usar, e categorias
separadas não trariam benefício de relatório sem também separar por `Motorização`.
