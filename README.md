# Coursely

Plataforma SaaS B2C para compartilhamento de cursos desejados. Este repositório está na **Fase 0.3**: pipeline base de qualidade no GitHub Actions + Docker/local stack + health check FE↔BE, seguindo [`agents.md`](agents.md) e [`roadmap.md`](roadmap.md).

## Pré-requisitos

- **.NET SDK 10**, versão de patch fixada em [`global.json`](global.json) (`dotnet --info` deve alinhar a essa entrada; o CI usa a mesma)
- **Node.js**: recomenda-se **20.19+** (algumas dependências do frontend declaram esse requisito; **20.18.x** costuma funcionar para build/test com Vite 6, mas pode gerar avisos `EBADENGINE`)
- **Docker Engine + Docker Compose v2** (para `docker compose` e para testes de integração com **Testcontainers**)

## Estrutura do repositório

```txt
src/
  Api/
  Application/
  Domain/
  Infrastructure/
  Shared/

tests/
  UnitTests/
  IntegrationTests/
  ArchitectureTests/

frontend/
```

- Backend segue **Clean Architecture** (`Domain` sem dependências de outras camadas).
- Frontend fica em **`frontend/`** (isolado da solution .NET).

## CI — GitHub Actions

O workflow [`quality.yml`](.github/workflows/quality.yml) roda em **push** para `main`/`master` e em **pull requests**.

| Job | O que valida |
|-----|----------------|
| `backend-quality` | `dotnet restore` / `dotnet build` / `dotnet test` em `Coursely.slnx` (Release), coleta cobertura com **Coverlet** (`XPlat Code Coverage`), inclui **ArchitectureTests**. Os testes **IntegrationTests** partilham uma única fixture com **Testcontainers** (SQL Server) para não levantar vários contentores em paralelo no runner. Artefato `dotnet-coverage` exige relatórios `*coverage*.xml` (Coverlet Cobertura cumpre o glob). Docker no runner executa cenários SQL; sem Docker ficam apenas skips. |
| `frontend-quality` | em `frontend/`: `npm ci`, `npm run format:check`, `npm run typecheck`, `npm run lint`, `npm run test`, `npm run build`. Node **20.19** em CI (`quality.yml`), alinhado ao [`engines`](frontend/package.json), ao [`frontend/.nvmrc`](frontend/.nvmrc) e ao `Dockerfile`. |

### Política de merge

Um PR não deve ser considerado pronto enquanto o workflow **Quality** falhar em qualquer job (build, testes, arquitetura, formatação, typecheck ou lint).

### Espelhar o CI localmente

Backend (na raiz):

```bash
dotnet restore Coursely.slnx
dotnet build Coursely.slnx --configuration Release
dotnet test Coursely.slnx --configuration Release --no-build --collect:"XPlat Code Coverage"
```

Frontend (`frontend/`):

```bash
cd frontend && npm ci
npm run format:check && npm run typecheck && npm run lint && npm run test && npm run build
```

Ou use [`scripts/test-all.sh`](scripts/test-all.sh): **rápido** (padrão) roda só `dotnet test` + Vitest verboso; **`./scripts/test-all.sh --full`** espelha o workflow (Release + Coverlet no backend e todas as etapas do `frontend-quality`).

## Backend (API)

Na raiz do repositório:

```bash
dotnet restore Coursely.slnx
dotnet build Coursely.slnx
dotnet run --project src/Api/Api.csproj
```

- Swagger (desenvolvimento): `/swagger`
- Health check: `GET /health`
- URL HTTP padrão (perfil `http`): `http://localhost:5230`
- **Sem `ConnectionStrings:DefaultConnection` configurada**: o host regista sempre **Identity** e serviços de auth — com CS usa **SQL Server**; sem CS usa EF **In-Memory** (**volátil**). Nos testes **`IntegrationTesting`**, após o SQL Server do Testcontainers iniciar também é aplicado **`ConnectionStrings__DefaultConnection`** no processo (`IntegrationTestWebApplicationFactory`), para que `AddInfrastructure` e o primeiro host usem a mesma instância. Para **persistência real** (Compose/prod), defina sempre a CS.


### Testes (.NET)

```bash
dotnet test Coursely.slnx
```

Inclui:

- **ArchitectureTests**: impede dependências indevidas na camada `Domain`
- **IntegrationTests**: `GET /health` via `WebApplicationFactory`; **uma** instância de SQL Server (Testcontainers) partilhada pela collection **`IntegrationTests`**, **Respawn** para limpar dados entre testes. Sem Docker/disponível, falhas só de infra resultam **skip**; falhas **após** o contentor subir (ex.: migrações) fazem falhar o suite.
- **UnitTests**: validadores e handlers de auth ([docs/auth-register.md](docs/auth-register.md), [docs/auth-login.md](docs/auth-login.md), [docs/auth-password-recovery.md](docs/auth-password-recovery.md)), `Result`, geração de JWT (`JwtTokenService`).

### Variáveis de ambiente (API)

| Variável | Obrigatória | Descrição |
|----------|-------------|-----------|
| `ConnectionStrings__DefaultConnection` | SQL Server persistente | Compose ou variável. Se omitida **fora** de `IntegrationTesting`, a API usa EF **In-Memory** (volátil); **defina sempre** para produção e dados reais. |
| `Cors__AllowedOrigins` | Recomendada em produção | Origens separadas por vírgula (ex.: `http://localhost:3000,http://localhost:5173`). |
| `Jwt__Key` | **Produção / qualquer ambiente com login JWT** | Segredo de assinatura do access token (HS256). Deve ser forte e longo o suficiente; ver `Jwt` em [`appsettings.json`](src/Api/appsettings.json). |
| `Jwt__Issuer` | Recomendada | Emissor do JWT (alinhado a `Jwt:Issuer` no appsettings). |
| `Jwt__Audience` | Recomendada | Audiência do JWT (`Jwt:Audience`). |
| `PasswordRecovery__FrontendBaseUrl` | Recomendada em Compose / resets reais | Base URL da SPA (**sem slash final**) para montar o link `…/redefinir-senha?userId=&token=` (logs locais ou e-mail futuro); ver [`docs/auth-password-recovery.md`](docs/auth-password-recovery.md). |

Endpoints de autenticação e contratos: [docs/auth-register.md](docs/auth-register.md), [docs/auth-login.md](docs/auth-login.md), [docs/auth-password-recovery.md](docs/auth-password-recovery.md).

Ver também [`appsettings.json`](src/Api/appsettings.json).

### Docker Compose (SQL Server, Redis, API, frontend)

1. Copie [`.env.example`](.env.example) para `.env` na raiz e defina **`MSSQL_SA_PASSWORD`** com uma senha forte (requisitos do SQL Server).
2. Opcional: ajuste **`VITE_API_URL`** — URL da API **como o navegador a enxerga** (padrão `http://localhost:5230`, alinhado ao mapeamento da API no Compose).

```bash
docker compose up --build
```

Serviços típicos:

| Serviço | URL / porta host |
|---------|------------------|
| Frontend (nginx) | `http://localhost:3000` |
| API | `http://localhost:5230` (`GET /health`, `/swagger`) |
| SQL Server | `localhost:1433` |
| Redis | `localhost:6379` |

`Dockerfile`s: [`src/Api/Dockerfile`](src/Api/Dockerfile), [`frontend/Dockerfile`](frontend/Dockerfile).

### Rodar todos os testes localmente (.NET + frontend)

Na raiz do repositório (requer **`dotnet`** e **`npm`** no PATH; para o frontend, rode `npm install` em [`frontend`](frontend/) na primeira vez):

```bash
./scripts/test-all.sh
```

Saídas: logs completos e resumo em `test-results/local/<timestamp>/` (ignored pelo Git), incluindo `REPORT.txt` e relatório no terminal ao final.

O script executa **os mesmos projetos .NET acima** e em seguida **`npm run test`** (Vitest) em `frontend/`.

## Frontend

```bash
cd frontend
npm install
npm run dev
```

Variáveis de ambiente (opcional):

- Copie [`frontend/.env.example`](frontend/.env.example) para `frontend/.env`
- `VITE_API_URL` aponta para a API (default: `http://localhost:5230`)

### Scripts úteis (`frontend/`)

```bash
npm run build        # typecheck + bundle produção
npm run test         # Vitest (happy-dom)
npm run lint         # ESLint
npm run format       # Prettier (write)
npm run format:check # Prettier (check)
npm run typecheck    # tsc -b
```

## Stack inicial (até Fase 0.3)

**Backend:** ASP.NET Core 10, MediatR, FluentValidation, AutoMapper 16, EF Core + SQL Server (`ApplicationDbContext` + migrations), Identity/JWT (pacotes preparados; fluxo na Fase 1), Serilog, Swagger, health checks (incl. banco quando há connection string), CORS, Docker.

**Frontend:** React 19, Vite 6, TypeScript strict, Tailwind CSS v4 + **shadcn/ui**, TanStack Query, Axios, Zustand (auth/tema/sessão — placeholders), React Hook Form + Zod, Framer Motion, Vitest + Testing Library; home consome `GET /health` da API.

## Design (Google Stitch + MCP)

Telas e fluxos de UI são alinhados ao projeto **[Google Stitch](https://stitch.withgoogle.com/projects/13905040481242586827)** ([Stitch — Design with AI](https://stitch.withgoogle.com/)), usando o **MCP Stitch** no Cursor quando disponível.

Documentação dedicada: [`docs/stitch.md`](docs/stitch.md).

## Próximas fases

- **1.x**: Autenticação e sessão (Identity, JWT, refresh) — ver [`roadmap.md`](roadmap.md).
