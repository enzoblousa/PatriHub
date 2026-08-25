# PatriHub — Especificação Funcional (MVP)

## 1. Visão geral
SaaS web onde o proprietário de imóveis e/ou carros alugados cadastra seus ativos, registra
manualmente receitas e despesas de cada um, e acompanha lucro, ROI e outras estatísticas para
decidir se vale a pena manter o ativo.

## 2. Personas / Papéis
| Papel | Descrição | Acesso |
|---|---|---|
| **User** | Dono do(s) ativo(s). Pessoa física com poucos imóveis/carros. | Vê e gerencia apenas os próprios ativos, contratos e lançamentos. |
| **Admin** | Operador da plataforma (backoffice PatriHub). | Gestão de usuários/suporte. **Não** tem acesso amplo a dados financeiros de terceiros por padrão — ver [Decisão Pendente #1](03-DECISOES-PENDENTES.md). |

Não há hierarquia adicional no MVP: um ativo pertence a exatamente um `User` (sem
co-propriedade — ver decisão pendente).

## 3. Glossário
- **Ativo**: termo genérico para Imóvel ou Carro cadastrado por um usuário.
- **Lançamento**: registro financeiro (receita ou despesa) associado a um ativo.
- **Contrato**: vínculo de locação entre um ativo e um locatário, com valor e vigência.
- **Locatário**: pessoa física que aluga o ativo do usuário.
- **Yield/ROI**: retorno percentual do ativo, calculado sobre o valor investido.

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
- Inadimplência: **Decisão Pendente #2** — como o status `Inadimplente` é definido (manual pelo
  usuário vs. automático ao passar do dia de vencimento sem lançamento de receita
  correspondente). Proposta de default: manual no MVP (usuário marca o contrato como
  inadimplente); automação fica para depois.

### 4.7 Lançamento Financeiro
- Id, AtivoId, Tipo (`Receita` | `Despesa`), Categoria, Valor, Data, Descrição (texto livre)
- Categorias de Receita (fixas no MVP): `Aluguel`, `TaxaDeServiço`, `MultaPorAtraso`, `Outras`
- Categorias de Despesa: **Decisão Pendente #3** — lista não fechada ainda. Proposta de
  default inicial: `IPTU`, `Condomínio`, `Manutenção`, `Seguro`, `IPVA`, `Multa`,
  `Financiamento`, `Imposto de Renda`, `Outras`.
- Sem anexos no MVP (apenas o valor é registrado, conforme resposta #20).

## 5. Regras de negócio — Cálculos financeiros
Todos os cálculos abaixo são **derivados** (não persistidos), recalculados a partir dos
lançamentos e dados do ativo.

- **Lucro do período** = Σ Receitas(período) − Σ Despesas(período), por ativo.
- **Lucro acumulado** = soma de todos os lançamentos desde a aquisição do ativo.
- **Depreciação** = ValorAquisição − ValorMercadoAtual (ambos informados manualmente pelo
  usuário; sem modelo de depreciação contábil automático no MVP).
- **ROI / Yield anualizado** = Lucro acumulado (ou anualizado) ÷ ValorAquisição (ou
  ValorMercadoAtual — a definir qual base usar). **Decisão Pendente #4.**
- **Custo de oportunidade** = ValorMercadoAtual × Taxa de referência anual (ex.: CDI/Selic),
  informada manualmente pelo usuário por enquanto (sem integração com índice oficial no MVP).
  **Decisão Pendente #5.**
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
- Como usuário, quero mudar o status de um ativo (Alugado/Vago/Manutenção/À venda).
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
- Como usuário, quero marcar um contrato como inadimplente.
- Regra: ao criar um contrato `Ativo`, o status do ativo correspondente deveria refletir
  `Alugado` automaticamente (a confirmar como regra de sincronização — ver Decisão Pendente
  #2).

### 6.4 Dashboard
- Como usuário, quero ver, por ativo: lucro do mês, lucro acumulado, ROI, comparação com
  outros ativos.
- Como usuário, quero ver a visão consolidada de todo o patrimônio (soma de todos os ativos):
  lucro total do mês, lucro total acumulado.
- Como usuário, quero ver uma projeção simples (ex.: lucro médio dos últimos N meses
  projetado) — critério exato de projeção é **Decisão Pendente #6**.
- (Fora do MVP, mas mapeado: comparativo detalhado entre ativos e outras métricas — resposta
  #25 deixa isso como "a decidir ao longo do projeto").

## 7. Regras de autorização
- `User` só enxerga/edita ativos, contratos, locatários e lançamentos onde `UsuárioId` é o
  próprio.
- `Admin` gerencia contas de usuário (ativar/desativar, suporte) mas, por padrão de
  privacidade (LGPD), **não lista dados financeiros de outros usuários** salvo necessidade de
  suporte explicitamente auditada — detalhar em Decisão Pendente #1.

## 8. Requisitos não-funcionais
- **LGPD**: consentimento de uso de dados no cadastro; dados pessoais de locatário (CPF)
  tratados como sensíveis; usuário pode solicitar exclusão da própria conta e dados.
- **Idioma/moeda**: pt-BR, valores em BRL, sem suporte a outra moeda no MVP.
- **Plataforma**: apenas web responsivo (sem app mobile nativo no MVP).
- **Acesso**: todo usuário tem o mesmo nível de acesso/limite (sem planos pagos ainda).

## 9. Itens em aberto
Ver [`03-DECISOES-PENDENTES.md`](03-DECISOES-PENDENTES.md) para a lista consolidada de
decisões que ficaram como "decidir depois" e as propostas de default sugeridas.
