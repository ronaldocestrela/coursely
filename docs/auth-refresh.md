# Autenticação — Refresh Token e Logout (Fase 1.3)

## Visão geral

- O login (`POST /api/auth/login`) devolve um par **access JWT** + **refresh opaco**.
- O refresh opaco é armazenado no banco apenas como **hash SHA-256 (Base64)**.
- **Rotação**: `POST /api/auth/refresh` valida o refresh atual, marca a linha como revogada, cria nova linha com novo hash e devolve novo par de tokens.
- **Sessões multi-dispositivo**: cada login começa uma **família** (`FamilyId`); logout e replay afetam apenas a família correta, sem derrubar outros logins válidos na mesma conta.
- **Detecção de reutilização**: se o cliente envia um refresh **já revogado** (ex.: cópia antiga após rotação ou após logout), tratamos como risco (`auth.refresh_token_reuse_detected`) e **revogamos todos os refresh tokens ativos dessa família**.

## Backend

### Endpoints

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| POST | `/api/auth/refresh` | Anónimo (sem Bearer obrigatório) | Troca refresh corrente por novo par (rotation). |
| POST | `/api/auth/logout` | Anónimo | Revoga o refresh token enviado (sessão atual). Corpo igual ao refresh. |

**Logout idempotente:** token desconhecido ou já revogado → **204 No Content** (segurança + UX).

### Códigos de erro (`refresh`)

| Code | HTTP | Situação |
|------|------|----------|
| `auth.refresh_token_invalid` | 401 | Hash não existe, formato inválido, conta bloqueada, etc. |
| `auth.refresh_token_expired` | 401 | `ExpiresAt` ultrapassado. |
| `auth.refresh_token_reuse_detected` | 401 | Token revogado reenviado; família suspensa no servidor. |

### Modelo `RefreshTokens` (migration `RefreshTokenRotation`)

- `FamilyId` (`uniqueidentifier`): linhagem iniciada em cada novo login isolado neste dispositivo/sessão.
- `RevokedAt` nullable: marca revogação (logout ou rotação).
- `ReplacedByTokenId` nullable FK: aponta para o token novo após rotação.
- `ReuseDetectedAt` nullable: marca quando houve replay de refresh revogado.
- Índice **único** em `TokenHash`.

### CQRS / Application

- `RefreshTokenCommand` + validator + handler → `IUserRefreshTokenService` (`UserRefreshTokenService`).
- `LogoutCommand` + validator + handler → `IUserLogoutService` (`UserLogoutService`).

## Frontend

Arquivos principais:

- [`frontend/src/services/http.ts`](../frontend/src/services/http.ts) — interceptor de **401**: tenta `POST /api/auth/refresh` **uma vez** por request usando `bareHttp` (Axios sem o interceptor para evitar recursão), com mutex para paralelismo; falha → `clearSession()` + `/login`.
- [`frontend/src/services/auth.ts`](../frontend/src/services/auth.ts) — `refreshSession(...)`, `logoutUser(...)` usando `bareHttp`.
- Tipos em [`frontend/src/types/authApi.ts`](../frontend/src/types/authApi.ts).

**Logout seguro**: dashboard chama API, faz `queryClient.clear()`, `clearSession()` e navega para `/login` (toast se o servidor falhar mas a sessão local ainda é limpa).

## Testes cobertos

### Backend unitário

- Validators de refresh / logout (`RefreshTokenCommandValidatorTests`, `LogoutCommandValidatorTests`).
- Handlers com mocks (`RefreshTokenCommandHandlerTests`, `LogoutCommandHandlerTests`).

### Backend integração (Testcontainers + SQL Server)

- Rotação com replay do token antigo e revogação em cadeia da família.
- Logout de uma sessão mantém outra sessão (dois clients, dois refresh distintos).

### Frontend

- `frontend/src/tests/logoutUser.test.ts` — garante `POST /api/auth/logout` via `bareHttp` (sem recursão com o interceptor).

## Referências

- [Login e JWT](./auth-login.md).
- Contratos API: [`src/Api/Controllers/AuthController.cs`](../src/Api/Controllers/AuthController.cs).
