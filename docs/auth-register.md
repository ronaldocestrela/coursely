# Registro de usuário (Fase 1.1)

## Endpoint

| Método | Rota | Descrição |
| ------- | ---- | --------- |
| `POST` | `/api/auth/register` | Cria conta com ASP.NET Identity |

### Corpo (`application/json`)

| Campo | Tipo | Regras |
| ----- | ---- | ------ |
| `name` | string | Obrigatório; máx. 200 caracteres (trim no servidor). |
| `email` | string | Obrigatório; formato de e-mail; máx. 256; único (normalizado pelo Identity). |
| `password` | string | Ver [Regras de senha](#regras-de-senha). |

### Respostas

- **201 Created** — corpo: `{ "id": "guid", "name": "...", "email": "..." }` (camelCase). Header `Location`: `/api/users/{id}`.
- **400 Bad Request** — falha de validação FluentValidation: `ValidationProblemDetails` (campos em `errors`).
- **409 Conflict** — e-mail duplicado: `{ "code": "auth.duplicate_email", "message": "..." }`.
- **400 Bad Request** — outros erros de criação (Identity): `{ "code": "auth.registration_failed", "message": "..." }`.

## Regras de senha

Alinhadas entre backend (`RegisterUserCommandValidator` + opções do Identity) e frontend (`registerFormSchema`):

1. Mínimo **8** caracteres (`RegisterUserCommandValidator.MinimumPasswordLength`).
2. Pelo menos **uma letra maiúscula** (`A-Z`).
3. Pelo menos **uma minúscula** (`a-z`).
4. Pelo menos **um dígito** (`0-9`).
5. Pelo menos **um caractere especial** (regex `[\W_]` no FluentValidation / frontend).

## Frontend

- Rota: `/cadastro`.
- Formulário: React Hook Form + Zod; envio via React Query (`registerUser` em `services/auth.ts`).
- Após sucesso: toast e redirecionamento para `/login`.

## Testes

- **Unitários**: validador, handler (mock do `IUserRegistrationService`).
- **Integração**: cadastro com sucesso e e-mail duplicado (requer Docker + Testcontainers SQL Server).
- **Frontend**: `frontend/src/tests/RegisterPage.test.tsx`.
