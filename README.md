# Coursely

Plataforma SaaS B2C para compartilhamento de cursos desejados. Este repositório está na **Fase 0.2**: Docker/local stack + health check FE↔BE, seguindo [`agents.md`](agents.md) e [`roadmap.md`](roadmap.md).

## Pré-requisitos

- **.NET SDK 10** (`dotnet --version` deve reportar `10.x`)
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

### Testes (.NET)

```bash
dotnet test Coursely.slnx
```

Inclui:

- **ArchitectureTests**: impede dependências indevidas na camada `Domain`
- **IntegrationTests**: `GET /health` via `WebApplicationFactory`; cenário com **SQL Server real em container (Testcontainers)** + **Respawn** para limpar dados. Se o Docker não estiver disponível, o teste que depende do SQL Server é **ignorado** (`Skipped`).
- **UnitTests**: exemplos sobre tipos compartilhados (`Result`)

### Variáveis de ambiente (API)

| Variável | Obrigatória | Descrição |
|----------|-------------|-----------|
| `ConnectionStrings__DefaultConnection` | Para Docker / EF | Connection string do SQL Server (Compose define por você). |
| `Cors__AllowedOrigins` | Recomendada em produção | Origens separadas por vírgula (ex.: `http://localhost:3000,http://localhost:5173`). |

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

## Stack inicial (até Fase 0.2)

**Backend:** ASP.NET Core 10, MediatR, FluentValidation, AutoMapper 16, EF Core + SQL Server (`ApplicationDbContext` + migrations), Identity/JWT (pacotes preparados; fluxo na Fase 1), Serilog, Swagger, health checks (incl. banco quando há connection string), CORS, Docker.

**Frontend:** React 19, Vite 6, TypeScript strict, Tailwind CSS v4 + **shadcn/ui**, TanStack Query, Axios, Zustand (auth/tema/sessão — placeholders), React Hook Form + Zod, Framer Motion, Vitest + Testing Library; home consome `GET /health` da API.

## Design (Google Stitch + MCP)

Telas e fluxos de UI são alinhados ao projeto **[Google Stitch](https://stitch.withgoogle.com/projects/13905040481242586827)** ([Stitch — Design with AI](https://stitch.withgoogle.com/)), usando o **MCP Stitch** no Cursor quando disponível.

Documentação dedicada: [`docs/stitch.md`](docs/stitch.md).

## Próximas fases

- **0.3**: CI (build, testes, cobertura, lint/typecheck).
