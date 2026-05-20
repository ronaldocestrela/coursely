# Coursely

Plataforma SaaS B2C para compartilhamento de cursos desejados. Este repositório está na **Fase 0.1**: fundação da solução (.NET + React), seguindo [`agents.md`](agents.md) e [`roadmap.md`](roadmap.md).

## Pré-requisitos

- **.NET SDK 10** (`dotnet --version` deve reportar `10.x`)
- **Node.js**: recomenda-se **20.19+** (algumas dependências do frontend declaram esse requisito; **20.18.x** costuma funcionar para build/test com Vite 6, mas pode gerar avisos `EBADENGINE`)

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
- **IntegrationTests**: valida `GET /health` via `WebApplicationFactory` (ambiente `IntegrationTesting`, sem redirect HTTPS que quebre o `HttpClient`)
- **UnitTests**: exemplos sobre tipos compartilhados (`Result`)

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

## Stack inicial (Fase 0.1)

**Backend:** ASP.NET Core 10, MediatR, FluentValidation, AutoMapper 16, EF Core + SQL Server + Identity (pacotes preparados; DbContext nas próximas fases), Serilog, Swagger, JWT Bearer (pacote referenciado; fluxo na Fase 1).

**Frontend:** React 19, Vite 6, TypeScript strict, Tailwind CSS v4 + **shadcn/ui**, TanStack Query, Axios, Zustand (auth/tema/sessão — placeholders), React Hook Form + Zod, Framer Motion, Vitest + Testing Library.

## Design (Google Stitch + MCP)

Telas e fluxos de UI são alinhados ao projeto **[Google Stitch](https://stitch.withgoogle.com/projects/13905040481242586827)** ([Stitch — Design with AI](https://stitch.withgoogle.com/)), usando o **MCP Stitch** no Cursor quando disponível.

Documentação dedicada: [`docs/stitch.md`](docs/stitch.md).

## Próximas fases (fora do escopo da 0.1)

- **0.2**: Docker Compose (SQL Server, Redis, API, frontend), integração mais próxima FE↔BE.
- **0.3**: CI (build, testes, cobertura, lint/typecheck).
