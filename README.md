# PatriHub

SaaS onde proprietários pessoa física de imóveis e/ou carros alugados acompanham o
desempenho financeiro do próprio patrimônio: lançamentos de receita/despesa por Ativo,
contratos de locação, status de inadimplência e métricas de Yield e ROI.

Veja o vocabulário de domínio completo em [`CONTEXT.md`](CONTEXT.md) e as decisões
arquiteturais em [`docs/adr/`](docs/adr).

## Stack

- **Backend**: .NET 10 / ASP.NET Core Web API, EF Core, PostgreSQL, autenticação JWT
- **Frontend**: Angular 21, servido em produção via Nginx
- **Infra local**: Docker Compose (Postgres + API + frontend)

## Estrutura do repositório

```
src/
  PatriHub.Api/             # Controllers, autenticação, composition root
  PatriHub.Application/     # Casos de uso (Ativos, Contratos, Lançamentos, Locatários, Dashboard, Admin)
  PatriHub.Domain/          # Entidades e cálculos de domínio (Yield, ROI, etc.)
  PatriHub.Infrastructure/  # Persistência (EF Core), Identity, JWT
tests/
  PatriHub.Domain.Tests/
  PatriHub.Api.IntegrationTests/
frontend/patrihub-web/      # App Angular
docs/
  adr/                      # Architecture Decision Records
  agents/                   # Convenções para agentes (issue tracker, labels, docs de domínio)
  spec/, design/
```

## Como rodar localmente

### Opção 1: Docker Compose (mais simples)

Sobe Postgres, API e frontend juntos:

```bash
docker compose up --build
```

- API: http://localhost:8080
- Frontend: http://localhost:4200

### Opção 2: rodando manualmente

**Pré-requisitos**: .NET 10 SDK, Node.js + npm, PostgreSQL 16 (local ou via `docker compose up postgres`).

**Backend**

```bash
dotnet restore
dotnet run --project src/PatriHub.Api
```

A API usa a connection string e os segredos definidos em
`src/PatriHub.Api/appsettings.Development.json` (valores padrão apontam para um Postgres
local `patrihub/patrihub`). Ajuste conforme seu ambiente — nunca commite segredos reais.

**Frontend**

```bash
cd frontend/patrihub-web
npm install
npm start
```

Abre em http://localhost:4200, consumindo a API em http://localhost:4200 conforme
configurado em `Cors:AllowedOrigins`.

## Testes

```bash
dotnet test
```

Frontend:

```bash
cd frontend/patrihub-web
npm test
```

## Contribuindo

Issues e specs vivem no GitHub Issues deste repositório (via `gh` CLI) — veja
[`docs/agents/issue-tracker.md`](docs/agents/issue-tracker.md) e
[`docs/agents/triage-labels.md`](docs/agents/triage-labels.md).

## Licença

[MIT](LICENSE)
