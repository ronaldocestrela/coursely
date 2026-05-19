# AGENTS.md — Plataforma SaaS B2C de Compartilhamento de Cursos Desejados

## Objetivo do Projeto

Construir uma plataforma SaaS B2C onde usuários possam:

* Criar conta e autenticar-se
* Adicionar cursos desejados
* Salvar:

  * Nome do curso
  * Descrição
  * Link de compra
  * Imagem/capa opcional
  * Categoria
* Compartilhar cursos com outros usuários
* Curtir/salvar cursos compartilhados
* Gerenciar sua própria biblioteca de cursos desejados
* Ter perfil público opcional
* Seguir outros usuários futuramente
* Operar em arquitetura escalável, segura e preparada para crescimento

---

# Stack Obrigatória

## Backend

* .NET 10
* ASP.NET Core Web API
* Clean Architecture
* CQRS
* MediatR
* Entity Framework Core
* SQL Server
* ASP.NET Identity
* JWT Bearer Authentication
* FluentValidation
* AutoMapper
* Serilog
* Redis (cache futuro)
* Swagger/OpenAPI
* Docker
* Docker Compose
* xUnit
* FluentAssertions
* Moq
* Testcontainers
* Respawn
* Minimal APIs ou Controllers (preferencialmente Controllers)

---

## Frontend

* React
* Vite
* TypeScript
* React Router DOM
* React Query (TanStack Query)
* Axios
* Zustand
* React Hook Form
* Zod
* TailwindCSS
* Shadcn/UI
* Framer Motion
* ESLint
* Prettier
* Vitest
* Testing Library

---

# Arquitetura Obrigatória

## Backend — Clean Architecture

Estrutura obrigatória:

```txt
src/
 ├── Api
 ├── Application
 ├── Domain
 ├── Infrastructure
 └── Shared

tests/
 ├── UnitTests
 ├── IntegrationTests
 └── ArchitectureTests
```

---

# Regras Arquiteturais

## Domain

Deve conter:

* Entities
* Value Objects
* Enums
* Domain Events
* Interfaces
* Regras de negócio puras

A camada Domain:

* NÃO pode depender de nenhuma outra camada
* NÃO pode depender de EF
* NÃO pode depender de ASP.NET
* NÃO pode depender de infraestrutura

---

## Application

Responsável por:

* CQRS
* Commands
* Queries
* Validators
* DTOs
* Interfaces
* Behaviors
* Casos de uso

Utilizar:

* MediatR
* FluentValidation
* Pipeline Behaviors

Cada funcionalidade deve seguir:

```txt
Feature/
 ├── Commands
 ├── Queries
 ├── DTOs
 ├── Validators
 └── Handlers
```

---

## Infrastructure

Responsável por:

* EF Core
* SQL Server
* Identity
* JWT
* Repositórios
* Serviços externos
* Cache
* File storage
* E-mail

---

## API

Responsável apenas por:

* Endpoints
* Middleware
* Configurações
* Autenticação
* Versionamento
* Swagger

---

# CQRS Obrigatório

Toda funcionalidade deve ser separada em:

## Commands

Operações de escrita:

* Create
* Update
* Delete
* Share
* Like
* Follow

## Queries

Operações de leitura:

* GetById
* GetAll
* Feed
* PublicProfile
* SearchCourses

---

# Banco de Dados

## SQL Server

Utilizar:

* EF Core Code First
* Migrations
* Configurations por entidade

Estrutura:

```txt
Infrastructure/
 └── Persistence/
      ├── Context
      ├── Configurations
      ├── Migrations
      └── Seed
```

---

# Identity

## Obrigatório usar ASP.NET Identity

Implementar:

* Registro
* Login
* Refresh Token
* Recuperação de senha
* Confirmação de e-mail
* Roles
* Claims
* Permissões granulares

---

# Autenticação

## JWT Bearer

Implementar:

* Access Token
* Refresh Token
* Rotação de refresh token
* Revogação
* Logout seguro

---

# Entidades Principais

## User

Campos mínimos:

```txt
Id
Name
Email
PasswordHash
Bio
AvatarUrl
IsPublicProfile
CreatedAt
UpdatedAt
```

---

## CourseWishlist

Representa lista de cursos desejados.

Campos:

```txt
Id
UserId
Title
Description
PurchaseLink
ThumbnailUrl
Category
Visibility
CreatedAt
UpdatedAt
```

---

## SharedCourse

```txt
Id
CourseWishlistId
SharedByUserId
SharedWithUserId
CreatedAt
```

---

## CourseLike

```txt
Id
CourseWishlistId
UserId
CreatedAt
```

---

# Funcionalidades MVP

## Autenticação

* Registro
* Login
* Logout
* Refresh token
* Recuperar senha

---

## Cursos Desejados

Usuário poderá:

* Criar curso
* Editar curso
* Remover curso
* Favoritar curso
* Compartilhar curso
* Tornar público/privado

---

## Feed Público

Exibir:

* Cursos compartilhados publicamente
* Cursos mais curtidos
* Cursos recentes

---

## Busca

Permitir busca por:

* Nome do curso
* Categoria
* Usuário

---

# Padrões Obrigatórios

## Repository Pattern

Somente quando necessário.

Preferir:

* DbContext direto nos handlers
* Queries otimizadas

---

## Result Pattern

Implementar padrão:

```txt
Result<T>
```

Para evitar exceptions como fluxo de negócio.

---

## Paginação

Toda listagem deve possuir:

* Page
* PageSize
* TotalCount

---

## Logging

Utilizar:

* Serilog
* CorrelationId
* Structured Logging

---

# TDD Obrigatório

## Nenhuma feature pode ser criada sem testes

Fluxo obrigatório:

1. Criar teste
2. Executar teste falhando
3. Implementar
4. Refatorar

---

# Testes Obrigatórios

## Unitários

Cobrir:

* Commands
* Queries
* Validators
* Domain Rules

---

## Integração

Cobrir:

* Banco de dados real
* API endpoints
* Identity
* JWT

Utilizar:

* Testcontainers
* SQL Server container

---

## Architecture Tests

Garantir:

* Dependências corretas
* Clean Architecture
* CQRS isolation

---

# Frontend — Estrutura

```txt
src/
 ├── app
 ├── pages
 ├── features
 ├── components
 ├── services
 ├── hooks
 ├── stores
 ├── routes
 ├── layouts
 ├── types
 ├── lib
 └── tests
```

---

# Frontend — Regras

## TypeScript Strict

Obrigatório:

```json
"strict": true
```

---

## Estado Global

Utilizar:

* Zustand

Somente para:

* Auth
* Tema
* Sessão

---

## Dados do Servidor

Utilizar:

* React Query

Nunca armazenar dados da API manualmente.

---

# Formulários

Utilizar:

* React Hook Form
* Zod

---

# UI/UX

Obrigatório:

* Responsivo
* Mobile First
* Acessibilidade
* Skeleton loading
* Dark mode
* Toast notifications
* Empty states
* Error boundaries

---

# Telas MVP

## Públicas

* Home
* Explorar cursos
* Login
* Cadastro

---

## Privadas

* Dashboard
* Minha lista
* Criar curso
* Editar curso
* Perfil
* Configurações

---

# Compartilhamento

Implementar:

* Compartilhamento via link público
* Compartilhamento entre usuários
* Privacidade:

  * Público
  * Privado
  * Compartilhado

---

# Segurança

Obrigatório:

* Rate limiting
* CORS configurado
* Proteção CSRF quando necessário
* Sanitização
* Validação server-side
* Headers de segurança
* Proteção contra brute force

---

# Docker

Obrigatório possuir:

## Backend

* Dockerfile
* docker-compose

## Frontend

* Dockerfile

## Serviços

* SQL Server
* Redis
* API
* Frontend

---

# CI/CD

Criar pipelines:

## Backend

* Build
* Testes
* Coverage
* Lint
* Publish

## Frontend

* Build
* Testes
* ESLint
* Typecheck

---

# Observabilidade

Implementar:

* Health Checks
* OpenTelemetry preparado
* Logs estruturados

---

# Documentação Viva

## Obrigatório atualizar:

* README.md
* ADRs
* Diagramas
* Fluxos
* Endpoints

A cada nova feature.

---

# Convenções

## Commits

Utilizar Conventional Commits:

```txt
feat:
fix:
refactor:
test:
docs:
chore:
```

---

# Proibições

## Nunca:

* Misturar regra de negócio com controller
* Utilizar lógica no frontend sem validação backend
* Utilizar repositories genéricos excessivos
* Ignorar testes
* Ignorar validação
* Utilizar DTOs anêmicos sem validação

---

# Critérios de Qualidade

## Backend

* SOLID
* Clean Code
* Baixo acoplamento
* Alta coesão
* Cobertura de testes relevante

---

## Frontend

* Componentização
* Responsividade
* Reutilização
* Performance
* Lazy loading
* Code splitting

---

# Roadmap Inicial

## Fase 1

* Setup solução
* Docker
* Identity
* JWT
* Clean Architecture
* CI/CD base

---

## Fase 2

* CRUD de cursos
* Compartilhamento
* Feed público
* Likes

---

## Fase 3

* Busca avançada
* Perfil público
* Sistema social

---

## Fase 4

* Notificações
* Recomendações
* Analytics

---

# Objetivo Final

Criar uma plataforma SaaS moderna, escalável e profissional, preparada para:

* Multi-tenant futuro
* Aplicativo mobile
* Escalabilidade horizontal
* Alta performance
* Crescimento de usuários
* Monetização futura via assinatura premium
