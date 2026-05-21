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

**Nota:** quando a API expuser JSON, o default do `System.Text.Json` para enums costuma ser **número**; alinhar com `JsonStringEnumConverter` ou mapear no cliente.

## Testes

- **Unitários (domínio)**: `tests/UnitTests/Courses/CourseWishlistTests.cs`.
- **Arquitetura**: `Domain` sem dependências proibidas (`tests/ArchitectureTests/DomainArchitectureTests.cs`).

Próximo: [Roadmap — Parte 2.2 Criar Curso](../roadmap.md) *(criar curso via API e formulário)*.
