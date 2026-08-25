# PatriHub — Especificação Funcional (MVP)

## 1. Visão geral
SaaS web onde o proprietário de imóveis e/ou carros alugados cadastra seus ativos, registra
manualmente receitas e despesas de cada um, e acompanha lucro, ROI e outras estatísticas para
decidir se vale a pena manter o ativo.

## 2. Personas / Papéis
| Papel | Descrição | Acesso |
|---|---|---|
| **User** | Dono do(s) ativo(s). Pessoa física com poucos imóveis/carros. | Vê e gerencia apenas os próprios ativos, contratos e lançamentos. |
| **Admin** | Operador da plataforma (backoffice PatriHub). | Gestão de usuários/suporte + leitura (nunca edição/exclusão) de ativos/lançamentos de qualquer usuário, com log de auditoria — ver [ADR-0002](../adr/0002-admin-acesso-leitura-com-auditoria.md). |

Não há hierarquia adicional no MVP: um ativo pertence a exatamente um `User` (sem
co-propriedade — ver decisão pendente).

## 3. Glossário
- **Ativo**: termo genérico para Imóvel ou Carro cadastrado por um usuário.
- **Lançamento**: registro financeiro (receita ou despesa) associado a um ativo.
- **Contrato**: vínculo de locação entre um ativo e um locatário, com valor e vigência.
- **Locatário**: pessoa física que aluga o ativo do usuário.
- **Yield**: retorno percentual apenas da renda de aluguel (receita líquida ÷ valor do ativo).
- **ROI**: retorno percentual total (lucro acumulado, incluindo valorização/depreciação),
  calculado sobre `ValorAquisição` e sobre `ValorMercadoAtual`. Ver [`../../CONTEXT.md`](../../CONTEXT.md).

## 4. Modelo de domínio (entidades e atributos)

### 4.1 Usuário
- Id, Nome, Email (único), SenhaHash, Role (`User` | `Admin`), CriadoEm

### 4.2 Ativo (conceito base — não é tabela própria, é a união de Imóvel/Carro)
Campos comuns a Imóvel e Carro:
- Id, UsuárioId (dono), Apelido/Nome do ativo, TipoAtivo (`Imovel` | `Carro`)
- Status (`Alugado`, `Vago`, `Manutenção`, `À venda`) — **exposto ao usuário**, conforme resposta #11
- DataAquisição, ValorAquisição, ValorMercadoAtual (o usuário atualiza manualmente)
- Financiado (bool)
- CriadoEm, AtualizadoEm

### 4.3 Imóvel (especialização de Ativo)
- Endereço (rua, número, complemento, bairro, cidade, UF, CEP)
- TipoImóvel (`Apartamento`, `Casa`, `Comercial`, `Terreno`)
- ÁreaM2
- Matrícula
- ValorIPTUMensal, ValorCondominioMensal
- Dados de financiamento (se Financiado = true): ValorParcela, SaldoDevedor, TaxaJurosAnual,
  ParcelasRestantes

### 4.4 Carro (especialização de Ativo)
- Placa, Marca, Modelo, AnoFabricação/AnoModelo
- ValorFIPEAtual (atualização manual — sem integração no MVP)
- KM atual
- ConsumoMedio (km/l)
- Dados de financiamento (mesma estrutura do imóvel, se Financiado = true)

### 4.5 Locatário
- Id, Nome, CPF, Telefone, Email, UsuárioId (a quem pertence o cadastro)

### 4.6 Contrato de Locação
- Id, AtivoId, LocatárioId, ValorAluguelMensal, DiaVencimento, DataInício, DataFim (opcional
  para contrato por prazo indeterminado), Status (`Ativo`, `Encerrado`, `Inadimplente`)
- Regra: um ativo só pode ter **um contrato `Ativo` por vez**.
- Inadimplência: automática, via job diário (`BackgroundService` in-process — ver
  [ADR-0003](../adr/0003-job-inadimplencia-background-service.md)). Um Contrato `Ativo` vira
  `Inadimplente` após 5 dias de carência sem um Lançamento (Receita, categoria Aluguel, mesmo
  `ContratoId`) dentro do mês de competência.

### 4.7 Lançamento Financeiro
- Id, AtivoId, ContratoId (**opcional/nullable** — vincula a receita ao contrato
  correspondente, usado pela detecção de inadimplência e por relatórios por locatário),
  Tipo (`Receita` | `Despesa`), Categoria, Valor, Data, Descrição (texto livre)
- Categorias de Receita (fixas no MVP): `Aluguel`, `TaxaDeServiço`, `MultaPorAtraso`, `Outras`
- Categorias de Despesa (fixas no MVP): `IPTU`, `Condomínio`, `Manutenção`, `Reforma`,
  `Seguro`, `IPVA`, `Multa`, `Financiamento`, `Administração`, `Imposto de Renda`, `Outras`.
  Customização pelo usuário fica para v2.
- Sem anexos no MVP (apenas o valor é registrado, conforme resposta #20).

## 5. Regras de negócio — Cálculos financeiros
Todos os cálculos abaixo são **derivados** (não persistidos), recalculados a partir dos
lançamentos e dados do ativo.

- **Lucro do período** = Σ Receitas(período) − Σ Despesas(período), por ativo.
- **Lucro acumulado** = soma de todos os lançamentos desde a aquisição do ativo.
- **Depreciação** = ValorAquisição − ValorMercadoAtual (ambos informados manualmente pelo
  usuário; sem modelo de depreciação contábil automático no MVP).
- **Yield** = Receita de aluguel líquida do período ÷ ValorMercadoAtual (retorno só de renda,
  sem valorização/depreciação).
- **ROI** = Lucro acumulado (incluindo valorização/depreciação) ÷ ValorAquisição **e** ÷
  ValorMercadoAtual — as duas bases são calculadas e exibidas lado a lado.
- **Custo de oportunidade** = ValorMercadoAtual × Taxa de referência anual (ex.: CDI/Selic),
  informada manualmente pelo usuário (sem integração com índice oficial no MVP, consistente
  com o Princípio 1 — manual antes de automático).
- **Imposto de Renda sobre aluguel**: entra como categoria de despesa lançada manualmente pelo
  usuário (o sistema não calcula a alíquota automaticamente no MVP).
- **Taxa de ocupação** (para exibição futura, fora do dashboard do MVP-relatórios): dias com
  contrato `Ativo` ÷ dias no período.

## 6. Casos de uso (User Stories) — Escopo do MVP
Prioridade conforme resposta #35: (a) cadastro de ativos, (b) lançamentos, (c) dashboard de
lucro, (d) contrato/locatário.

### 6.1 Cadastro de Ativos
- Como usuário, quero cadastrar um imóvel com seus dados, para acompanhar seu desempenho.
- Como usuário, quero cadastrar um carro com seus dados, para acompanhar seu desempenho.
- Como usuário, quero editar/atualizar valor de mercado de um ativo, para manter o ROI
  correto.
- Como usuário, quero marcar manualmente um ativo como `Manutenção` ou `À venda`
  (`Alugado`/`Vago` são definidos automaticamente pelo ciclo de vida do contrato — ver 6.3).
- Como usuário, quero excluir um ativo (soft delete, mantendo histórico financeiro).
- Como usuário, quero listar todos os meus ativos com visão resumida (tipo, status, lucro do
  mês).

### 6.2 Lançamentos Financeiros
- Como usuário, quero lançar uma receita para um ativo (categoria, valor, data, descrição).
- Como usuário, quero lançar uma despesa para um ativo.
- Como usuário, quero editar ou excluir um lançamento.
- Como usuário, quero listar/filtrar lançamentos por ativo, período e tipo.

### 6.3 Contratos e Locatários
- Como usuário, quero cadastrar um locatário (nome, CPF, contato).
- Como usuário, quero criar um contrato de locação vinculando um locatário a um ativo, com
  valor de aluguel e vigência.
- Como usuário, quero encerrar um contrato.
- Contrato `Inadimplente` é marcado automaticamente pelo sistema (não é uma ação do usuário
  — ver 4.6).
- Regra de sincronização (semi-automática): ao criar um contrato `Ativo`, o `Status` do ativo
  correspondente muda para `Alugado` automaticamente; ao encerrar o contrato, volta para
  `Vago` automaticamente. `Manutenção` e `À venda` continuam sendo definidos manualmente pelo
  usuário a qualquer momento (e prevalecem até o próximo evento de contrato).

### 6.4 Dashboard
- Como usuário, quero ver, por ativo: lucro do mês, lucro acumulado, Yield, ROI (sobre
  ValorAquisição e sobre ValorMercadoAtual), comparação com outros ativos.
- Como usuário, quero ver a visão consolidada de todo o patrimônio (soma de todos os ativos):
  lucro total do mês, lucro total acumulado.
- Como usuário, quero ver uma projeção simples: média de lucro dos últimos 3 meses,
  projetada linearmente.
- (Fora do MVP, mas mapeado: comparativo detalhado entre ativos e outras métricas — resposta
  #25 deixa isso como "a decidir ao longo do projeto").

## 7. Regras de autorização
- `User` só enxerga/edita ativos, contratos, locatários e lançamentos onde `UsuárioId` é o
  próprio.
- `Admin` gerencia contas de usuário (ativar/desativar, suporte) e tem **leitura** (nunca
  edição/exclusão) de ativos e lançamentos de qualquer usuário, para dar suporte sem precisar
  de acesso ao banco. Todo acesso do Admin a dado de outro usuário é registrado em log de
  auditoria (quem, quando, qual usuário/recurso) — ver
  [ADR-0002](../adr/0002-admin-acesso-leitura-com-auditoria.md).

## 8. Requisitos não-funcionais
- **LGPD**: consentimento de uso de dados no cadastro; dados pessoais de locatário (CPF)
  tratados como sensíveis; usuário pode solicitar exclusão da própria conta e dados.
- **Idioma/moeda**: pt-BR, valores em BRL, sem suporte a outra moeda no MVP.
- **Plataforma**: apenas web responsivo (sem app mobile nativo no MVP).
- **Acesso**: todo usuário tem o mesmo nível de acesso/limite (sem planos pagos ainda).

## 9. Itens em aberto
Todas as decisões que estavam pendentes foram resolvidas — ver
[`03-DECISOES-PENDENTES.md`](03-DECISOES-PENDENTES.md) para o histórico, [`../../CONTEXT.md`](../../CONTEXT.md)
para o glossário e [`../adr/`](../adr/) para as decisões arquiteturais registradas. Único
item ainda propositalmente em aberto: **identidade visual/referência de design**, a
resolver quando chegarmos na etapa de UI.
