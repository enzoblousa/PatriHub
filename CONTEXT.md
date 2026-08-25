# PatriHub

SaaS onde proprietários pessoa física de imóveis e/ou carros alugados acompanham o
desempenho financeiro do próprio patrimônio.

## Language

**Ativo**:
Termo genérico para Imóvel ou Carro cadastrado por um usuário; unidade básica de
acompanhamento financeiro (um Ativo tem um único dono).
_Avoid_: Propriedade, Bem

**Lançamento**:
Registro financeiro de receita ou despesa associado a um Ativo, e opcionalmente a um
Contrato (via `ContratoId` nullable, para rastrear a qual contrato uma receita de aluguel
corresponde).
_Avoid_: Transação, Movimentação

**Contrato (de Locação)**:
Vínculo de locação entre um Ativo e um Locatário, com valor de aluguel e vigência. Um
Ativo só pode ter um Contrato `Ativo` por vez. A existência de um Contrato `Ativo` dirige
automaticamente o `Status` do Ativo para "Alugado" (ver Status do Ativo).
_Avoid_: Aluguel (esse é o valor pago, não o vínculo)

**Locatário**:
Pessoa física que aluga o Ativo de um usuário.
_Avoid_: Inquilino, Cliente

**Status do Ativo**:
Campo semi-automático com os valores `Alugado`, `Vago`, `Manutenção`, `À venda`. Criar um
Contrato `Ativo` seta automaticamente "Alugado"; encerrar o contrato reverte para "Vago".
"Manutenção" e "À venda" só são definidos manualmente pelo usuário.

**Yield**:
Retorno percentual **apenas da renda** gerada por um Ativo no período (receita de aluguel
líquida ÷ valor do ativo). Não inclui valorização/depreciação do ativo.
_Avoid_: usar "ROI" para essa métrica — são números diferentes

**ROI**:
Retorno percentual **total** de um Ativo: lucro acumulado (incluindo valorização ou
depreciação) sobre a base de investimento. Calculado duas vezes — sobre `ValorAquisição` e
sobre `ValorMercadoAtual` — e exibido lado a lado, por serem leituras diferentes.
_Avoid_: usar "Yield" para essa métrica

**Inadimplente (status do Contrato)**:
Atribuído automaticamente por um job periódico: um Contrato `Ativo` vira `Inadimplente`
quando passam 5 dias de carência após o vencimento sem um Lançamento tipo Receita,
categoria Aluguel, com esse `ContratoId`, dentro do mês de competência.
