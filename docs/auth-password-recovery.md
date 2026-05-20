# Recuperação de senha (Fase 1.4)

Fluxo ponta-a-ponta com **ASP.NET Identity** (`GeneratePasswordResetTokenAsync` / `ResetPasswordAsync`), CQRS (`ForgotPasswordCommand`, `ResetPasswordCommand`), **`Result`** no reset e UX **enumeration-safe** no “esqueci senha”.

## Endpoints

| Método | Rota | Auth | Descrição |
| ------ | ---- | ---- | ---------- |
| `POST` | `/api/auth/forgot-password` | Não | Solicita recuperação; resposta sempre genérica para não revelar se o e-mail existe. |
| `POST` | `/api/auth/reset-password` | Não | Aplica novo segredo com token emitido pelo Identity. |

### `POST /api/auth/forgot-password`

Corpo (`application/json`):

| Campo | Tipo | Regras |
| ----- | ---- | ------ |
| `email` | string | Obrigatório; e-mail válido (FluentValidation, alinhado ao cadastro). |

Respostas:

- **200 OK** — `{ "message": "..." }`, **sempre a mesma mensagem** tanto para e-mail cadastrado quanto não cadastrado.
- **400 Bad Request** — `ValidationProblemDetails`.

Comportamento de negócio:

- Para e-mail encontrado: gera token de reset, monta um link público `{FrontendBaseUrl}/redefinir-senha?userId=<guid>&token=<url-encoded>` e dispara **`IPasswordResetEmailSender`**.
- Para e-mail não encontrado: não faz nada perceptível pelo cliente (**mesmo 200/message**).

### `POST /api/auth/reset-password`

Corpo (`application/json`, camelCase):

| Campo | Tipo | Regras |
| ----- | ---- | ------ |
| `userId` | GUID | Obrigatório; deve ser diferente de `Guid.Empty`. |
| `token` | string | Obrigatório — token cru retornado pelo Identity (o fragmento já vem **URL-encoded** no link). |
| `password` / `confirmPassword` | string | Igual à política forte do cadastro (`RegisterUserCommandValidator`); `confirmPassword` deve coincidir (`ResetPasswordCommandValidator`). |

Respostas:

- **204 No Content** — senha atualizada com sucesso.
- **400 Bad Request** — validação FluentValidation **ou** `{ "code", "message" }` quando token/usuário inválido/expirado.
  - Códigos típicos: `auth.password_reset_invalid`, `auth.password_reset_failed`.

## E-mail vs ambiente local

A infraestrutura atual inclui `PasswordResetLoggingEmailSender`:

- Escreve **log estruturado Serilog** com `RecipientEmail`, `UserId` e `ResetLink`.
- Produção deve trocar a implementação de `IPasswordResetEmailSender` por um gateway SMTP/SendGrid/Mailgun preservando os contratos Application.

## Configuração (`PasswordRecovery` em appsettings / env)

| Chave | Descrição |
| ----- | ---------- |
| `PasswordRecovery:FrontendBaseUrl` | URL pública da SPA (**sem trailing slash**) usada para montar deep link de reset. Defaults em [`appsettings.json`](../src/Api/appsettings.json). |

Variável de ambiente (Docker / Linux): **`PasswordRecovery__FrontendBaseUrl`**.

Compose (fluxo navegador ➝ frontend em `:3000`) recomenda configurar esse valor como `http://localhost:3000` para que o usuário cole o link do log diretamente no browser.

Arquivos de referência:

- [`UserPasswordRecoveryService`](../src/Infrastructure/Identity/UserPasswordRecoveryService.cs)
- [`PasswordResetLoggingEmailSender`](../src/Infrastructure/Identity/PasswordResetLoggingEmailSender.cs)
- [`Infrastructure/DependencyInjection`](../src/Infrastructure/DependencyInjection.cs)

## Frontend

Rotas públicas:

- **`/esqueci-senha`** — formulário apenas de e-mail; React Hook Form + Zod + mutation React Query (`forgotPassword` em [`services/auth.ts`](../frontend/src/services/auth.ts)).
- **`/redefinir-senha?userId=...&token=...`** — lê query string, valida com Zod, envia novo segredo (`resetPassword`).

`services/http.ts` inclui `/api/auth/forgot-password` e `/api/auth/reset-password` na lista de rotas onde **401 não dispara refresh** (evita recursões indevidas caso o servidor retorne esse status futuramente).

Tela [`LoginPage`](../frontend/src/pages/LoginPage.tsx) exibe link “Esqueci minha senha”.

## Fluxo

```txt
SPA /login
   └─► POST /auth/forgot-password  (enumeration-safe response)
           └─► Identity.GeneratePasswordResetToken
           └─► Log estruturado com deep link (/redefinir-senha?userId=&token=)
SPA /redefinir-senha
   └─► POST /auth/reset-password  (Identity.ResetPasswordAsync)
```

## Erros novos (`AuthErrorCodes`)

- `auth.password_reset_invalid`
- `auth.password_reset_failed`

## Testes

- **API Unit**: `ForgotPasswordCommandValidator`, `ResetPasswordCommandValidator`, handlers com `IUserPasswordRecoveryService`.
- **API Integration**: `PasswordRecoveryIntegrationTests` (mensagem igual para desconhecido vs conhecido, reset feliz-path com token válido gerado pelo `UserManager` no test harness, falha por token aleatório). Requer Docker + SQL Testcontainers quando habilitados.
- **Frontend**: [`ForgotPasswordPage.test.tsx`](../frontend/src/tests/ForgotPasswordPage.test.tsx), [`ResetPasswordPage.test.tsx`](../frontend/src/tests/ResetPasswordPage.test.tsx), [`passwordRecovery.auth.test.ts`](../frontend/src/tests/passwordRecovery.auth.test.ts).

## Documentação relacionada

- [Registro](auth-register.md)
- [Login](auth-login.md)
- [Refresh / Logout](auth-refresh.md)
