# Curso desejado — modelo de domínio (Fase 2.1)

Esta fase introduz a entidade **`CourseWishlist`** (lista de cursos que o usuário deseja fazer), persistência via EF Core e componentes visuais base no frontend. **Não há endpoints HTTP nesta parte**; APIs (`POST /api/courses`, etc.) entram na **Fase 2.2+**.

## Domínio (`Domain.Courses`)

| Tipo | Descrição |
| ---- | ----------- |
| `CourseVisibility` | `Private` (0), `Public` (1), `Shared` (2) |
| `CourseWishlist` | Agregado com `Id`, `UserId`, `Title`, `Description`, `PurchaseLink`, `ThumbnailUrl`, `Category`, `Visibility`, `CreatedAt`, `UpdatedAt` |

### Regras de negócio

- **Título** obrigatório (após trim); máximo **300** caracteres.
- **`UserId`** não pode ser `Guid.Empty` (curso pertence a um usuário).
- **Link de compra** (`PurchaseLink`): opcional; se informado, deve ser URL **absoluta** `http` ou `https` (após trim).
- **Thumbnail** (`ThumbnailUrl`): opcional; se informado, mesma validação de URL http(s).
- **`UpdatedAt`** é atualizado em `UpdateDetails` quando o conteúdo muda.

O domínio **não** referencia ASP.NET Identity nem EF: apenas `Guid UserId`.

## Persistência

- Tabela **`CourseWishlists`** com FK para **`AspNetUsers`**, delete em cascata.
- `Visibility` persistido como **`int`** (enum).
- Índices: `UserId`; composto `(UserId, CreatedAt)` para listagens futuras.

Migration: `AddCourseWishlist` em `src/Infrastructure/Persistence/Migrations/`.

## Frontend

- Tipos: `frontend/src/types/courseApi.ts` (`Course`, `CourseVisibility` em strings PascalCase para UI).
- Componentes: `frontend/src/components/course/` — `CourseCard`, `CourseBadge`, `CourseVisibilityLabel`.
- Teste: `frontend/src/tests/CourseCard.test.tsx`.

**Nota:** a API registra `JsonStringEnumConverter` em [`src/Api/Program.cs`](../src/Api/Program.cs) para serializar/deserializar `CourseVisibility` como string (`"Private"`, `"Public"`, `"Shared"`) no JSON, alinhado ao frontend (`frontend/src/types/courseApi.ts`).

---

# Fase 2.2 — Criar curso (`POST /api/courses`)

Permite que um usuário autenticado crie um registro na tabela `CourseWishlists`. O **`userId` vem apenas do JWT** (`ClaimTypes.NameIdentifier`); o body não aceita `userId`.

## Autenticação

- **Obrigatório:** header `Authorization: Bearer <access_token>`.
- Sem token válido: **401 Unauthorized**.

## Request

`POST /api/courses`

Body JSON (camelCase):

| Campo | Tipo | Obrigatório | Observação |
| ----- | ---- | ----------- | ---------- |
| `title` | string | sim | trim; máx. 300 caracteres |
| `description` | string \| null | não | máx. 4000 |
| `purchaseLink` | string \| null | não | se informado, URL absoluta `http` ou `https` |
| `thumbnailUrl` | string \| null | não | idem |
| `category` | string \| null | não | máx. 200 |
| `visibility` | string | sim | `"Private"`, `"Public"` ou `"Shared"` |

## Respostas

- **201 Created** — corpo com o curso criado; header `Location: /api/courses/{id}`.
- **400 Bad Request** — validação FluentValidation (`ValidationProblemDetails`) ou falha de domínio mapeada (`course.invalid_input`).
- **401 Unauthorized** — sem autenticação.

Exemplo de corpo de sucesso (campos principais): `id`, `userId`, `title`, `description`, `purchaseLink`, `thumbnailUrl`, `category`, `visibility`, `createdAt`, `updatedAt`.

## Backend (CQRS)

- Command: `CreateCourseCommand` + `CreateCourseCommandValidator` + `CreateCourseCommandHandler`.
- Persistência: `ICourseCreator` / `CourseCreator` (EF `ApplicationDbContext`).
- Testes: `CreateCourseCommandValidatorTests`, `CreateCourseCommandHandlerTests`, `CreateCourseIntegrationTests`.

## Frontend

- Página privada: `/cursos/novo` — [`CreateCoursePage`](../frontend/src/pages/CreateCoursePage.tsx).
- Formulário: React Hook Form + Zod ([`courseFormSchema`](../frontend/src/features/courses/courseFormSchema.ts)); mutation React Query via [`createCourse`](../frontend/src/services/courses.ts).
- Após sucesso, redireciona para **`/dashboard`** até existir a rota **Minha lista** (Fase 2.3).
- Atalho no painel: botão **Adicionar curso** no [`DashboardPage`](../frontend/src/pages/DashboardPage.tsx).

## Testes

- **Unitários (domínio)**: `tests/UnitTests/Courses/CourseWishlistTests.cs`.
- **Unitários (criação)**: `tests/UnitTests/CreateCourseCommandValidatorTests.cs`, `tests/UnitTests/CreateCourseCommandHandlerTests.cs`.
- **Integração**: `tests/IntegrationTests/CreateCourseIntegrationTests.cs` (requer Docker para SQL Server / Testcontainers).
- **Arquitetura**: `Domain` sem dependências proibidas (`tests/ArchitectureTests/DomainArchitectureTests.cs`).
- **Frontend**: `frontend/src/tests/CreateCoursePage.test.tsx`.

Próximo: [Roadmap — Parte 2.3 Minha Lista de Cursos](../roadmap.md).
