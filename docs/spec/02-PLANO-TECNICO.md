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
- **Mensageria/fila:** não necessária no MVP (sem notificações, sem processamento
  assíncrono) — reavaliar se/quando notificações entrarem no roadmap

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

Lancamentos (Id, AtivoId FK, Tipo, Categoria, Valor, Data, Descricao, CriadoEm)
```
Estratégia de herança Imóvel/Carro: **table-per-type** — tabela `Ativos` com os campos
comuns + tabela filha (`Imoveis` ou `Carros`) com PK = FK para `Ativos.Id` (EF Core:
`TPT inheritance` mapeado a partir de uma classe base `Ativo`).

## 4. Autenticação e autorização
- Registro/login via email + senha (ASP.NET Core Identity).
- Emissão de JWT (access token) — sem refresh token complexo no MVP (reavaliar).
- Roles: `User`, `Admin`, aplicadas via `[Authorize(Roles = "...")]` nos controllers.
- Toda query de dados do domínio filtra implicitamente por `UsuarioId` do token (nunca por
  parâmetro vindo do cliente), para evitar vazamento entre contas.

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
