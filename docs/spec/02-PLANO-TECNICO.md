# PatriHub — Plano Técnico (MVP)

## 1. Stack
- **Backend:** .NET 10 (LTS) — ASP.NET Core Web API
- **Frontend:** Angular (versão estável mais recente no início da implementação)
- **ORM:** Entity Framework Core
- **Banco de dados:** PostgreSQL 16+
- **Autenticação:** ASP.NET Core Identity + JWT (self-hosted, sem provedor externo)
- **Containers:** Docker + Docker Compose (dev local); imagens usadas em produção
- **Hospedagem:** Azure (App Service ou Container Apps) + Azure Database for PostgreSQL
- **CI/CD:** GitHub Actions (build, testes, push de imagem, deploy) — default, sem
  restrição informada pelo usuário
- **Mensageria/fila:** não necessária no MVP. Única exceção é o job diário de detecção de
  inadimplência, que roda como `BackgroundService` in-process — sem fila/agendador dedicado
  (ver [ADR-0003](../adr/0003-job-inadimplencia-background-service.md)).

## 2. Arquitetura
Monólito modular em camadas (Clean Architecture simplificada), evitando
microsserviços no estágio de validação:

```
PatriHub.sln
├── src/
│   ├── PatriHub.Domain/          # Entidades, enums, regras de negócio puras
│   ├── PatriHub.Application/     # Casos de uso, DTOs, interfaces (CQRS leve ou services)
│   ├── PatriHub.Infrastructure/  # EF Core, repositórios, Identity, migrations
│   └── PatriHub.Api/             # Controllers, autenticação JWT, composição/DI
├── frontend/
│   └── patrihub-web/             # Angular app (standalone components, feature-based folders)
├── tests/
│   ├── PatriHub.Domain.Tests/
│   └── PatriHub.Application.Tests/
├── docker-compose.yml            # api + postgres + frontend (dev)
└── docs/spec/                    # esta especificação
```

## 3. Modelo de dados (visão relacional)

```
Usuarios (Id, Nome, Email, SenhaHash, Role, CriadoEm)

Ativos (Id, UsuarioId FK, Apelido, TipoAtivo, Status, DataAquisicao,
        ValorAquisicao, ValorMercadoAtual, Financiado, CriadoEm, AtualizadoEm)

Imoveis (AtivoId FK/PK, Endereco*, TipoImovel, AreaM2, Matricula,
         ValorIptuMensal, ValorCondominioMensal)

Carros (AtivoId FK/PK, Placa, Marca, Modelo, AnoFabricacao, AnoModelo,
        ValorFipeAtual, Km, ConsumoMedio)

DadosFinanciamento (AtivoId FK/PK, ValorParcela, SaldoDevedor, TaxaJurosAnual,
                     ParcelasRestantes)

Locatarios (Id, UsuarioId FK, Nome, Cpf, Telefone, Email)

Contratos (Id, AtivoId FK, LocatarioId FK, ValorAluguelMensal, DiaVencimento,
           DataInicio, DataFim, Status)

Lancamentos (Id, AtivoId FK, ContratoId FK nullable, Tipo, Categoria, Valor, Data, Descricao,
             CriadoEm)

AuditLogsAdmin (Id, AdminUsuarioId FK, UsuarioAlvoId FK, Recurso, RecursoId, CriadoEm)
```
Estratégia de herança Imóvel/Carro: **table-per-type** — tabela `Ativos` com os campos
comuns + tabela filha (`Imoveis` ou `Carros`) com PK = FK para `Ativos.Id` (EF Core:
`TPT inheritance` mapeado a partir de uma classe base `Ativo`).

## 4. Autenticação e autorização
- Registro/login via email + senha (ASP.NET Core Identity).
- Emissão de JWT (access token) com validade longa (ex.: 7 dias) — sem refresh token no MVP
  (ver [ADR-0001](../adr/0001-sem-refresh-token-mvp.md)).
- Roles: `User`, `Admin`, aplicadas via `[Authorize(Roles = "...")]` nos controllers.
- Toda query de dados do domínio de `User` filtra implicitamente por `UsuarioId` do token
  (nunca por parâmetro vindo do cliente), para evitar vazamento entre contas.
- `Admin` tem endpoints de leitura (somente leitura) sobre ativos/lançamentos de qualquer
  usuário; cada chamada grava uma linha em `AuditLogsAdmin` (ver
  [ADR-0002](../adr/0002-admin-acesso-leitura-com-auditoria.md)).
- Job diário (`BackgroundService`) varre Contratos `Ativo` e marca `Inadimplente` os que
  passaram 5 dias de carência sem Lançamento correspondente (ver
  [ADR-0003](../adr/0003-job-inadimplencia-background-service.md)).

## 5. Ambientes
- **Dev local:** Docker Compose subindo API + PostgreSQL + Angular dev server.
- **Produção:** Azure App Service (ou Container Apps) + Azure Database for PostgreSQL
  Flexible Server.
- Sem ambiente de staging formal no MVP (avaliar depois se necessário).

## 6. Convenções gerais
- Migrations do EF Core versionadas no repositório.
- Testes unitários mínimos para regras de cálculo financeiro (lucro, ROI) desde o início,
  por serem o core-value do produto.
- Nomeclatura de API em português nos DTOs voltados ao domínio de negócio (ex.:
  `Ativo`, `Lancamento`) para manter consistência com a spec funcional; nomes técnicos de
  infraestrutura em inglês, seguindo convenção .NET padrão.

## 7. Fora do plano técnico do MVP
- Sem mensageria/fila, sem cache distribuído, sem multi-região, sem CDN dedicado, sem
  integrações externas (conforme `00-CONSTITUTION.md`).

## 8. Frontend (Angular)
- **Componentes:** standalone (sem `NgModule`), pasta por feature (`ativos/`, `lancamentos/`,
  `locatarios-contratos/`, `dashboard/`, `admin/`, `auth/`) — mesmo espírito modular do backend
  (`PatriHub.Domain`/`Application`/`Infrastructure`/`Api`), mas sem replicar as quatro camadas
  no frontend: aqui um `service` por feature já concentra chamada HTTP + estado.
- **Estado:** Angular Signals (`signal`/`computed`) dentro dos services de feature, sem NgRx ou
  outra lib de state management — a superfície é CRUD + um dashboard de leitura, não uma app
  com estado compartilhado complexo o bastante pra justificar a dependência extra (mesmo
  Princípio 2 da Constituição, "simples antes de completo").
- **Roteamento:** `provideRouter`, com `authGuard` (exige JWT válido em `localStorage`) e
  `adminGuard` (exige a claim de Role `Admin` — ver `PatriHubClaimTypes`/`ClaimTypes.Role` no
  token) protegendo as rotas correspondentes; redireciona pro login quando o guard barra.
- **Chamada à API:** um `HttpInterceptorFn` único injeta `Authorization: Bearer <token>` em
  toda request (token lido de `localStorage` — ver [ADR-0004](../adr/0004-jwt-em-localstorage.md))
  e trata `401` de forma centralizada (limpa o token, redireciona pro login) — nenhum service de
  feature repete essa lógica.
- **Forms:** Reactive Forms em todo formulário (Ativo, Lançamento, Locatário, Contrato,
  login/registro) — não template-driven, pela validação tipada e testável fora do template.
- **CORS:** `PatriHub.Api` ainda não tem policy de CORS configurada — necessário liberar a
  origin do dev server Angular via `AddCors`/`UseCors` em `Program.cs` antes do frontend
  conseguir chamar a API localmente.
- **Docker:** `docker-compose.yml` ganha um serviço `frontend` (dev server ou build servido —
  a decidir na implementação), somando-se aos serviços `api`/`postgres` já existentes.
- **Testes:** convenção padrão do Angular CLI mais recente no início da implementação
  (Karma+Jasmine ou Vitest) para componentes/services; `HttpTestingController` mocka o
  `HttpClient` nos testes de service que chamam a API — sem Testcontainers aqui, já que não há
  banco no frontend.
- **Identidade visual:** propositalmente fora deste plano técnico (ver `01-SPEC-FUNCIONAL.md
  §9`) — paleta, tipografia e estilo geral são decididos tela a tela na implementação, não
  fixados de antemão.
