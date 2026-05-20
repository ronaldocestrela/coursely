# Login com JWT (Fase 1.2)

## Endpoints

| Método | Rota | Auth | Descrição |
| ------ | ---- | ---- | ---------- |
| `POST` | `/api/auth/login` | Não | Valida credenciais (ASP.NET Identity) e retorna access JWT + refresh token opaco persistido. |
| `GET` | `/api/auth/me` | Bearer JWT | Retorna `userId`, `email`, `name`, `roles` extraídos das claims do token. |

### `POST /api/auth/login`

Corpo (`application/json`):

| Campo | Tipo | Regras |
| ----- | ---- | ------ |
| `email` | string | Obrigatório; e-mail; máx. 256 (FluentValidation). |
| `password` | string | Obrigatório. |

Respostas:

- **200 OK** — JSON (camelCase típico do ASP.NET Core) com:
  - `userId` (GUID)
  - `name`, `email`
  - `roles` (array de strings; vazio se o usuário não tiver papéis)
  - `accessToken` (JWT HS256)
  - `accessTokenExpiresAt` (ISO 8601)
  - `refreshToken` (string opaca; **armazenada apenas como hash SHA-256 no banco**; rotação/revogação implementadas na **Fase 1.3** — ver [auth-refresh.md](auth-refresh.md))
  - `refreshTokenExpiresAt` (ISO 8601)
- **400 Bad Request** — `ValidationProblemDetails` (validação FluentValidation).
- **401 Unauthorized** — `{ "code": "auth.invalid_credentials", "message": "..." }` para credenciais incorretas ou usuário inexistente (mensagem genérica).

### `GET /api/auth/me`

Header: `Authorization: Bearer <accessToken>`.

- **200 OK** — `{ "userId", "email", "name", "roles" }`.
- **401 Unauthorized** — token ausente ou inválido.

## JWT

- **Algoritmo**: HS256 (simétrico).
- **Claims principais** (entre outras): `sub` (user id), `email`, `name`; `role` repetido por papel (via `ClaimTypes.Role`). Também são emitidos `NameIdentifier`, `Email` e `Name` alinhados ao ASP.NET.
- **Validação** (`JwtBearer`): `Issuer`, `Audience`, assinatura, lifetime.

## Configuração (`Jwt` em appsettings / env)

| Chave | Descrição |
| ----- | ----------- |
| `Jwt:Key` | Segredo UTF-8 para assinatura (deve ser suficientemente longo para HS256; em desenvolvimento ver `appsettings.Development.json`). **Obrigatório** para autenticação JWT: sem chave, `AddInfrastructure` não registra `AddJwtBearer` (API pode subir, mas login/`/me` não validam token). |
| `Jwt:Issuer` | Emissor do token. |
| `Jwt:Audience` | Audiência do token. |
| `Jwt:AccessTokenExpirationMinutes` | TTL do access token. |
| `Jwt:RefreshTokenExpirationDays` | TTL do refresh token persistido. |

Variáveis de ambiente (ex.: Docker): `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience`, etc.

## Frontend

- Rota: `/login` — React Hook Form + Zod; mutation React Query (`loginUser` em `services/auth.ts`).
- Sessão: Zustand (`useAppStore`) com **persist** em `localStorage` (`coursely-storage`); campos `accessToken`, `refreshToken`, `user`.
- Axios (`services/http.ts`): interceptor **request** com Bearer; interceptor **response** em **401** renova sessão uma vez via `POST /api/auth/refresh` (ver [auth-refresh.md](auth-refresh.md)).
- Rota privada: `/dashboard` protegida por `RequireAuth`.

## Testes

- **Unitários**: `LoginCommandValidator`, `LoginCommandHandler`, `JwtTokenService` (claims); frontend `LoginPage.test.tsx`.
- **Integração**: login + `/api/auth/me` + senha inválida (Docker + Testcontainers SQL Server).

Ver também [Registro de usuário (Fase 1.1)](auth-register.md), [Refresh / Logout — rotação e revogação (Fase 1.3)](auth-refresh.md) e rotas `POST /api/auth/refresh`, `POST /api/auth/logout` documentadas lá.

