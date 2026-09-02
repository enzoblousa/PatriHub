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
- **Produção**: Render (API) + Neon (Postgres) + Cloudflare Pages (frontend) — ver [Deploy](#deploy)

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

## Deploy

Versão beta em produção, rodando 100% em tiers grátis (sem cartão de crédito em nenhuma
perna):

- **Frontend**: https://patrihub.pages.dev
- **API**: https://patrihub-api-3lz1.onrender.com (`/health` pra smoke test rápido)

- **[Render](https://render.com)** — API (`src/PatriHub.Api/Dockerfile`), free web service.
  Definido como Blueprint em [`render.yaml`](render.yaml).
- **[Neon](https://neon.tech)** — Postgres gerenciado, free tier. Nunca deleta dado — só
  suspende o compute quando ocioso (retoma automaticamente na próxima conexão). Janela de
  restauração point-in-time: 6 horas.
- **[Cloudflare Pages](https://pages.cloudflare.com)** — build estático do Angular
  (`frontend/patrihub-web`), free tier, bandwidth ilimitada.

### Variáveis de ambiente obrigatórias no Render

`render.yaml` declara todas com `sync: false` — o dashboard do Render pede cada uma na
primeira sincronização do blueprint (ou na criação manual do serviço):

| Variável | Valor |
|---|---|
| `ConnectionStrings__PatriHubDb` | Connection string do Neon, em formato ADO.NET: `Host=...;Port=5432;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true` (Neon fornece no formato `postgresql://user:pass@host/db?sslmode=require` — precisa converter) |
| `Jwt__Secret` | Segredo novo gerado só para produção (ex.: `openssl rand -base64 48`) — nunca o valor de dev do `appsettings.json` |
| `AdminBootstrap__Email` | Email real da primeira conta Admin |
| `AdminBootstrap__Senha` | Senha forte real — nunca o valor de dev do `appsettings.json` |
| `Cors__AllowedOrigins__0` | URL do Cloudflare Pages: `https://patrihub.pages.dev` (sem barra no final — CORS compara a origem de forma exata) |
| `Cors__AllowedOrigins__1` | URL do domínio próprio (`https://app.patrihub.com.br`), quando apontado — array, convive com `__0` durante a transição (ver ADR-0009) |
| `Frontend__BaseUrl` | Origem pública do frontend, usada só pra montar o link do email de recuperação de senha (ver ADR-0009) — hoje `https://patrihub.pages.dev`, trocar para o domínio próprio quando o DNS estiver apontado |
| `Resend__ApiKey` | Chave de API do [Resend](https://resend.com), com o domínio `patrihub.com.br` verificado (SPF/DKIM) — sem essa variável, o backend cai no fallback que só loga o link em vez de enviar o email de verdade (ver `EnviadorDeEmailConsole`, ADR-0009) |

Os demais valores (`ASPNETCORE_ENVIRONMENT`, `SeedDadosDemo=false`, `Jwt__Issuer`/`Audience`/
`ExpiraEmDias`) já vêm fixados no `render.yaml`, não-secretos.

Os valores de dev commitados em `src/PatriHub.Api/appsettings.json` continuam ali de
propósito — são usados por `dotnet run`/`dotnet test` local e são sempre sobrescritos em
produção pelas variáveis de ambiente acima (que têm precedência sobre `appsettings.json`
independente do `ASPNETCORE_ENVIRONMENT`). Não reescrevemos o histórico do Git para remover
os segredos de dev antigos — uma vez rotacionados, os valores antigos não dão acesso a nada.

### CI/CD

Dois workflows em [`.github/workflows/`](.github/workflows/), cada um só disparado por
mudanças na sua área (`src/**`+`tests/**` para o backend, `frontend/**` para o frontend):

1. Todo push/PR roda os testes (`dotnet test`, `ng test`).
2. Só em push na `main`, depois dos testes passarem: o job de deploy do backend chama o
   **Deploy Hook** do Render (`RENDER_DEPLOY_HOOK_URL` nos GitHub Secrets) — o Auto-Deploy do
   Render está **desligado** de propósito, então nenhum deploy acontece sem os testes
   passarem primeiro. O job de deploy do frontend builda e publica no Cloudflare Pages via
   `cloudflare/pages-action` (`CLOUDFLARE_API_TOKEN`/`CLOUDFLARE_ACCOUNT_ID` nos GitHub
   Secrets).

### Limitações conhecidas desta beta

- Sem confirmação de email nem reset de senha por email — se alguém travar, o Admin reseta a
  senha manualmente pelo painel.
- Sem backup além da janela de 6h de point-in-time restore do Neon.
- Sem domínio próprio — usa os subdomínios grátis dos hosts (`*.onrender.com`,
  `*.pages.dev`).
- O free tier do Render "dorme" após 15 minutos sem tráfego; o primeiro acesso depois disso
  pode levar de 30 a 60 segundos para responder.

## Contribuindo

Issues e specs vivem no GitHub Issues deste repositório (via `gh` CLI) — veja
[`docs/agents/issue-tracker.md`](docs/agents/issue-tracker.md) e
[`docs/agents/triage-labels.md`](docs/agents/triage-labels.md).

## Licença

[MIT](LICENSE)
