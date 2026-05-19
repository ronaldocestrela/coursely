# Roadmap de Construção — Coursely

Este roadmap transforma o planejamento do `agents.md` em uma sequência de pequenas entregas completas. Cada etapa deve gerar uma parte utilizável da aplicação, com backend, frontend, testes e documentação suficientes para evoluir sem perder qualidade.

## Princípios de Execução

- Cada parte deve ser implementada em ciclo completo: domínio, API, banco, testes, tela, integração frontend e documentação.
- Toda feature deve seguir TDD: teste falhando, implementação, refatoração.
- Backend deve manter Clean Architecture, CQRS, MediatR, FluentValidation, EF Core e ASP.NET Identity.
- Frontend deve usar React, Vite, TypeScript strict, React Query, Zustand apenas para sessão/tema/auth, React Hook Form, Zod, TailwindCSS e Shadcn/UI.
- Nenhuma regra de negócio deve ficar no controller ou somente no frontend.
- Toda listagem deve nascer paginada.
- Toda entrega deve atualizar a documentação viva quando criar endpoints, fluxos ou decisões relevantes.

## Estrutura Geral de Entregas

Cada item do roadmap deve ser considerado pronto apenas quando cumprir:

- Backend implementado com command/query, validator, handler, DTOs e endpoint.
- Banco atualizado com migration quando necessário.
- Testes unitários para domínio, validators, commands e queries relevantes.
- Testes de integração para endpoint, banco e autenticação quando aplicável.
- Frontend implementado com página, componentes, formulário, estados de loading, erro e empty state.
- Integração via service/API client e React Query.
- Validação frontend com Zod e validação backend com FluentValidation.
- Documentação atualizada com endpoints, fluxo e observações técnicas.

---

# Fase 0 — Fundação do Projeto

Objetivo: criar uma base executável, testável e preparada para evolução.

## Parte 0.1 — Estrutura do Repositório

### Backend

- Criar solução .NET 10.
- Criar projetos:
  - `src/Api`
  - `src/Application`
  - `src/Domain`
  - `src/Infrastructure`
  - `src/Shared`
  - `tests/UnitTests`
  - `tests/IntegrationTests`
  - `tests/ArchitectureTests`
- Configurar referências respeitando Clean Architecture:
  - `Domain` sem dependências externas.
  - `Application` dependendo de `Domain` e `Shared`.
  - `Infrastructure` dependendo de `Application`, `Domain` e `Shared`.
  - `Api` dependendo de `Application`, `Infrastructure` e `Shared`.
- Adicionar pacotes base:
  - MediatR
  - FluentValidation
  - AutoMapper
  - Entity Framework Core
  - SQL Server provider
  - ASP.NET Identity
  - JWT Bearer
  - Serilog
  - Swagger/OpenAPI
  - xUnit, FluentAssertions, Moq, Testcontainers, Respawn.

### Frontend

- Criar aplicação React com Vite e TypeScript.
- Ativar `strict: true`.
- Configurar TailwindCSS.
- Configurar Shadcn/UI.
- Configurar ESLint e Prettier.
- Criar estrutura:
  - `src/app`
  - `src/pages`
  - `src/features`
  - `src/components`
  - `src/services`
  - `src/hooks`
  - `src/stores`
  - `src/routes`
  - `src/layouts`
  - `src/types`
  - `src/lib`
  - `src/tests`
- Configurar React Router DOM.
- Configurar React Query.
- Configurar Zustand para auth, tema e sessão.

### Testes

- Criar teste de arquitetura garantindo que `Domain` não dependa de nenhuma camada.
- Criar teste básico de health check da API.
- Criar teste básico de renderização da aplicação frontend.

### Documentação

- Criar `README.md` com instruções para rodar backend, frontend e testes.
- Documentar estrutura inicial.

### Critério de Aceite

- Backend sobe localmente.
- Frontend sobe localmente.
- Testes iniciais passam.
- Arquitetura base impede dependências indevidas.

## Parte 0.2 — Docker e Ambiente Local

### Backend

- Criar `Dockerfile` da API.
- Criar `docker-compose.yml` com:
  - SQL Server
  - Redis
  - API
  - Frontend
- Configurar connection string por variável de ambiente.
- Configurar health checks da API.

### Frontend

- Criar `Dockerfile` do frontend.
- Configurar variável de ambiente para URL da API.
- Criar tela simples consumindo o health check.

### Testes

- Garantir que testes de integração usem Testcontainers com SQL Server.
- Garantir que banco de testes seja limpo com Respawn.

### Documentação

- Documentar comandos de Docker.
- Documentar variáveis de ambiente obrigatórias.

### Critério de Aceite

- `docker compose up` inicia banco, cache, API e frontend.
- Frontend consegue consultar health check da API.
- Testes de integração usam banco real em container.

## Parte 0.3 — Pipeline Base de Qualidade

### Backend

- Criar workflow de CI para:
  - Restore
  - Build
  - Testes
  - Coverage
  - Validação arquitetural

### Frontend

- Criar workflow de CI para:
  - Install
  - Typecheck
  - ESLint
  - Testes
  - Build

### Documentação

- Registrar no README como o CI valida o projeto.

### Critério de Aceite

- Pipeline executa backend e frontend.
- Nenhum merge deve ser considerado pronto sem build e testes passando.

---

# Fase 1 — Autenticação e Sessão

Objetivo: entregar fluxo completo de conta, login, JWT, refresh token e proteção de rotas.

## Parte 1.1 — Registro de Usuário

### Backend

- Criar entidade de usuário baseada em ASP.NET Identity.
- Adicionar campos:
  - `Name`
  - `Bio`
  - `AvatarUrl`
  - `IsPublicProfile`
  - `CreatedAt`
  - `UpdatedAt`
- Criar command `RegisterUserCommand`.
- Criar validator com regras:
  - Nome obrigatório.
  - E-mail válido.
  - Senha forte.
- Criar endpoint `POST /api/auth/register`.
- Retornar resultado padronizado com `Result<T>`.

### Frontend

- Criar página `Cadastro`.
- Criar formulário com React Hook Form e Zod.
- Criar campos:
  - Nome
  - E-mail
  - Senha
  - Confirmação de senha
- Exibir validações client-side.
- Integrar com API via mutation do React Query.
- Exibir toast de sucesso ou erro.
- Redirecionar para login após cadastro.

### Testes

- Unitários para validator de registro.
- Unitários para command handler.
- Integração para cadastro com sucesso.
- Integração para e-mail duplicado.
- Frontend: teste de renderização e validação do formulário.

### Documentação

- Documentar endpoint de registro.
- Documentar regras de senha.

### Critério de Aceite

- Usuário consegue criar conta pela tela.
- API bloqueia dados inválidos.
- E-mail duplicado retorna erro de negócio.

## Parte 1.2 — Login com JWT

### Backend

- Criar command `LoginCommand`.
- Validar credenciais com ASP.NET Identity.
- Gerar access token JWT.
- Gerar refresh token persistido.
- Criar endpoint `POST /api/auth/login`.
- Incluir claims básicas:
  - UserId
  - Email
  - Name
  - Roles

### Frontend

- Criar página `Login`.
- Criar formulário com e-mail e senha.
- Salvar sessão no Zustand.
- Configurar Axios para incluir bearer token.
- Criar rota protegida.
- Redirecionar usuário autenticado para dashboard.

### Testes

- Unitários para geração de token.
- Integração para login com sucesso.
- Integração para senha inválida.
- Frontend: teste de submit, erro e redirecionamento.

### Documentação

- Documentar contrato de login.
- Documentar formato dos tokens.

### Critério de Aceite

- Usuário consegue logar e acessar rota privada.
- Token é enviado automaticamente nas chamadas autenticadas.
- Login inválido mostra erro claro.

## Parte 1.3 — Refresh Token e Logout

### Backend

- Criar entidade/tabela de refresh token se necessário.
- Implementar rotação de refresh token.
- Implementar revogação.
- Criar endpoints:
  - `POST /api/auth/refresh`
  - `POST /api/auth/logout`
- Invalidar refresh token no logout.

### Frontend

- Configurar interceptor Axios para renovar token quando access token expirar.
- Implementar logout seguro.
- Limpar store de auth.
- Redirecionar para login.

### Testes

- Integração para refresh token válido.
- Integração para refresh token reutilizado/revogado.
- Integração para logout.
- Frontend: teste de limpeza de sessão.

### Documentação

- Documentar fluxo de renovação.
- Documentar regra de revogação.

### Critério de Aceite

- Sessão é renovada sem quebrar UX.
- Logout impede reutilização do refresh token.

## Parte 1.4 — Recuperação de Senha

### Backend

- Criar command `ForgotPasswordCommand`.
- Criar command `ResetPasswordCommand`.
- Preparar serviço de e-mail via interface na Application e implementação na Infrastructure.
- Em ambiente local, registrar link/token em log estruturado.
- Criar endpoints:
  - `POST /api/auth/forgot-password`
  - `POST /api/auth/reset-password`

### Frontend

- Criar página `Esqueci minha senha`.
- Criar página `Redefinir senha`.
- Exibir feedback sem vazar se e-mail existe.

### Testes

- Unitários para validators.
- Integração para solicitar recuperação.
- Integração para reset com token válido e inválido.
- Frontend: testes dos formulários.

### Documentação

- Documentar fluxo de recuperação.
- Registrar decisão sobre envio de e-mail local.

### Critério de Aceite

- Usuário consegue iniciar recuperação.
- Usuário consegue redefinir senha com token válido.

---

# Fase 2 — Cursos Desejados

Objetivo: permitir que o usuário crie, edite, remova, visualize e organize sua lista de cursos desejados.

## Parte 2.1 — Modelo de Curso Desejado

### Backend

- Criar entidade `CourseWishlist`.
- Campos:
  - `Id`
  - `UserId`
  - `Title`
  - `Description`
  - `PurchaseLink`
  - `ThumbnailUrl`
  - `Category`
  - `Visibility`
  - `CreatedAt`
  - `UpdatedAt`
- Criar enum `CourseVisibility`:
  - `Private`
  - `Public`
  - `Shared`
- Criar configuração EF Core.
- Criar migration.
- Criar regras de domínio:
  - Título obrigatório.
  - Link de compra válido quando informado.
  - Curso pertence a um usuário.

### Frontend

- Criar tipos TypeScript para curso.
- Criar componentes visuais base:
  - `CourseCard`
  - `CourseBadge`
  - `CourseVisibilityLabel`

### Testes

- Unitários para regras de domínio.
- Architecture tests para dependências.
- Frontend: teste de renderização do card.

### Documentação

- Documentar entidade e regras.

### Critério de Aceite

- Modelo de curso existe no domínio e banco.
- Frontend possui componentes base reutilizáveis.

## Parte 2.2 — Criar Curso

### Backend

- Criar command `CreateCourseCommand`.
- Criar validator.
- Criar handler.
- Criar endpoint `POST /api/courses`.
- Proteger endpoint com JWT.
- Associar curso ao usuário autenticado.

### Frontend

- Criar página `Criar Curso`.
- Criar formulário com:
  - Título
  - Descrição
  - Link de compra
  - Imagem/capa opcional
  - Categoria
  - Visibilidade
- Integrar mutation com React Query.
- Exibir loading, erro e sucesso.
- Redirecionar para `Minha lista`.

### Testes

- Unitários para validator.
- Unitários para handler.
- Integração para criação autenticada.
- Integração bloqueando usuário não autenticado.
- Frontend: teste de formulário e submit.

### Documentação

- Documentar endpoint de criação.
- Documentar contrato de visibilidade.

### Critério de Aceite

- Usuário autenticado consegue criar curso pela tela.
- Curso criado aparece associado ao usuário.

## Parte 2.3 — Minha Lista de Cursos

### Backend

- Criar query `GetMyCoursesQuery`.
- Implementar paginação:
  - `page`
  - `pageSize`
  - `totalCount`
- Criar filtros iniciais:
  - Categoria
  - Visibilidade
- Criar endpoint `GET /api/courses/my`.

### Frontend

- Criar página `Minha lista`.
- Exibir cards paginados.
- Criar estados:
  - Skeleton loading
  - Empty state
  - Error state
- Adicionar filtros por categoria e visibilidade.

### Testes

- Unitários para query handler.
- Integração para paginação.
- Integração garantindo que usuário só veja seus cursos.
- Frontend: teste de loading, empty state e lista populada.

### Documentação

- Documentar endpoint de listagem.
- Documentar paginação padrão.

### Critério de Aceite

- Usuário visualiza apenas seus próprios cursos.
- Listagem funciona com paginação e filtros.

## Parte 2.4 — Editar Curso

### Backend

- Criar query `GetMyCourseByIdQuery`.
- Criar command `UpdateCourseCommand`.
- Validar propriedade do curso.
- Criar endpoints:
  - `GET /api/courses/{id}`
  - `PUT /api/courses/{id}`

### Frontend

- Criar página `Editar Curso`.
- Reaproveitar formulário de curso.
- Carregar dados existentes.
- Atualizar cache do React Query após edição.
- Exibir feedback de sucesso.

### Testes

- Unitários para validator e handler.
- Integração para edição com sucesso.
- Integração bloqueando edição de curso de outro usuário.
- Frontend: teste de carregar e salvar edição.

### Documentação

- Documentar endpoints de detalhe e atualização.

### Critério de Aceite

- Usuário edita apenas seus próprios cursos.
- Interface reflete alteração sem recarregar manualmente.

## Parte 2.5 — Remover Curso

### Backend

- Criar command `DeleteCourseCommand`.
- Validar propriedade do curso.
- Decidir entre soft delete ou delete físico via ADR.
- Criar endpoint `DELETE /api/courses/{id}`.

### Frontend

- Adicionar ação de remover na lista.
- Criar modal de confirmação.
- Atualizar cache após remoção.
- Exibir toast de sucesso ou erro.

### Testes

- Unitários para handler.
- Integração para remoção com sucesso.
- Integração bloqueando remoção de curso de outro usuário.
- Frontend: teste do modal e remoção.

### Documentação

- Registrar ADR de soft delete ou delete físico.
- Documentar endpoint de remoção.

### Critério de Aceite

- Usuário remove curso pela interface.
- Curso removido deixa de aparecer na lista.

## Parte 2.6 — Dashboard Inicial

### Backend

- Criar query `GetDashboardSummaryQuery`.
- Retornar:
  - Total de cursos.
  - Total públicos.
  - Total privados.
  - Cursos recentes.
- Criar endpoint `GET /api/dashboard/summary`.

### Frontend

- Criar página `Dashboard`.
- Exibir cards de resumo.
- Exibir cursos recentes.
- Adicionar atalhos para criar curso e abrir minha lista.

### Testes

- Unitários para query.
- Integração para resumo por usuário.
- Frontend: teste de renderização do dashboard.

### Documentação

- Documentar endpoint de resumo.

### Critério de Aceite

- Dashboard mostra informações reais do usuário autenticado.

---

# Fase 3 — Compartilhamento, Feed Público e Likes

Objetivo: permitir descoberta e interação social básica com cursos.

## Parte 3.1 — Tornar Curso Público ou Privado

### Backend

- Criar command `UpdateCourseVisibilityCommand`.
- Regras:
  - Apenas dono pode alterar visibilidade.
  - Curso público aparece no feed.
  - Curso privado não aparece publicamente.
- Criar endpoint `PATCH /api/courses/{id}/visibility`.

### Frontend

- Adicionar controle de visibilidade no card e edição.
- Exibir badge clara de público/privado/compartilhado.
- Atualizar cache após alteração.

### Testes

- Unitários para regra de visibilidade.
- Integração para alteração de visibilidade.
- Integração garantindo que curso privado não aparece no feed.
- Frontend: teste de alteração visual.

### Documentação

- Documentar regras de visibilidade.

### Critério de Aceite

- Usuário controla a privacidade do curso.
- Cursos privados permanecem invisíveis publicamente.

## Parte 3.2 — Feed Público

### Backend

- Criar query `GetPublicFeedQuery`.
- Listar cursos públicos paginados.
- Ordenações:
  - Recentes
  - Mais curtidos
- Criar endpoint `GET /api/feed`.

### Frontend

- Criar página `Explorar cursos`.
- Exibir feed com paginação.
- Criar seletor de ordenação.
- Exibir empty state quando não houver cursos.

### Testes

- Unitários para query.
- Integração para feed paginado.
- Integração para ordenação por recentes.
- Frontend: teste de feed com loading e dados.

### Documentação

- Documentar endpoint de feed.

### Critério de Aceite

- Visitantes e usuários autenticados conseguem explorar cursos públicos.

## Parte 3.3 — Curtir e Descurtir Curso

### Backend

- Criar entidade `CourseLike`.
- Criar command `LikeCourseCommand`.
- Criar command `UnlikeCourseCommand`.
- Regras:
  - Usuário autenticado pode curtir curso público.
  - Usuário não pode curtir o mesmo curso duas vezes.
- Criar endpoints:
  - `POST /api/courses/{id}/like`
  - `DELETE /api/courses/{id}/like`

### Frontend

- Adicionar botão de like no `CourseCard`.
- Exibir contador de likes.
- Usar optimistic update com rollback em caso de erro.
- Bloquear ou redirecionar visitante não autenticado para login.

### Testes

- Unitários para regras de like.
- Integração para curtir.
- Integração para impedir like duplicado.
- Integração para descurtir.
- Frontend: teste do botão e contador.

### Documentação

- Documentar endpoints de like.

### Critério de Aceite

- Usuário autenticado curte e descurte cursos.
- Contador permanece consistente.

## Parte 3.4 — Compartilhamento por Link Público

### Backend

- Criar query `GetPublicCourseByIdQuery`.
- Criar endpoint `GET /api/public/courses/{id}`.
- Garantir que apenas cursos públicos sejam acessíveis.

### Frontend

- Criar página pública de detalhe do curso.
- Criar botão `Copiar link`.
- Exibir dados do curso, autor, categoria e link de compra.

### Testes

- Integração para acessar curso público.
- Integração bloqueando curso privado.
- Frontend: teste de página pública e copiar link.

### Documentação

- Documentar URL pública de curso.

### Critério de Aceite

- Curso público pode ser compartilhado por link.
- Curso privado não é acessível via link público.

## Parte 3.5 — Compartilhamento Entre Usuários

### Backend

- Criar entidade `SharedCourse`.
- Criar command `ShareCourseWithUserCommand`.
- Criar query `GetSharedWithMeCoursesQuery`.
- Regras:
  - Apenas dono compartilha.
  - Usuário destino precisa existir.
  - Curso compartilhado aparece para o usuário destino.
- Criar endpoints:
  - `POST /api/courses/{id}/share`
  - `GET /api/courses/shared-with-me`

### Frontend

- Criar modal de compartilhamento.
- Buscar usuário por e-mail ou nome.
- Criar seção `Compartilhados comigo`.
- Exibir estado de compartilhamento.

### Testes

- Unitários para command.
- Integração para compartilhar com usuário existente.
- Integração bloqueando compartilhamento por não dono.
- Frontend: teste do modal e lista compartilhada.

### Documentação

- Documentar fluxo de compartilhamento privado.

### Critério de Aceite

- Usuário compartilha curso com outro usuário.
- Destinatário visualiza cursos compartilhados com ele.

---

# Fase 4 — Busca e Perfil Público

Objetivo: melhorar descoberta de cursos e usuários.

## Parte 4.1 — Busca de Cursos

### Backend

- Criar query `SearchCoursesQuery`.
- Permitir busca por:
  - Nome do curso
  - Categoria
  - Usuário autor
- Retornar apenas cursos públicos para busca pública.
- Criar endpoint `GET /api/search/courses`.

### Frontend

- Criar campo de busca global.
- Criar página de resultados.
- Permitir filtros por categoria.
- Exibir destaque para termo buscado quando simples de implementar.

### Testes

- Unitários para query.
- Integração para busca por título.
- Integração para busca por categoria.
- Integração garantindo exclusão de cursos privados.
- Frontend: teste de busca e resultados.

### Documentação

- Documentar parâmetros de busca.

### Critério de Aceite

- Usuário encontra cursos públicos por nome, categoria ou autor.

## Parte 4.2 — Perfil do Usuário

### Backend

- Criar query `GetMyProfileQuery`.
- Criar command `UpdateMyProfileCommand`.
- Campos editáveis:
  - Nome
  - Bio
  - AvatarUrl
  - Perfil público ou privado
- Criar endpoints:
  - `GET /api/profile/me`
  - `PUT /api/profile/me`

### Frontend

- Criar página `Perfil`.
- Criar formulário de edição.
- Atualizar sessão local quando nome/avatar mudar.
- Exibir preview do perfil.

### Testes

- Unitários para validator.
- Integração para carregar perfil.
- Integração para atualizar perfil.
- Frontend: teste de formulário de perfil.

### Documentação

- Documentar endpoints de perfil.

### Critério de Aceite

- Usuário edita seu perfil.
- Alterações aparecem na interface.

## Parte 4.3 — Perfil Público

### Backend

- Criar query `GetPublicProfileQuery`.
- Retornar:
  - Nome
  - Bio
  - Avatar
  - Cursos públicos do usuário
- Respeitar `IsPublicProfile`.
- Criar endpoint `GET /api/users/{id}/public-profile`.

### Frontend

- Criar página pública de perfil.
- Exibir cursos públicos do usuário.
- Exibir empty state quando não houver cursos públicos.
- Exibir mensagem adequada quando perfil não for público.

### Testes

- Integração para perfil público.
- Integração bloqueando perfil privado.
- Frontend: teste de perfil público.

### Documentação

- Documentar contrato do perfil público.

### Critério de Aceite

- Perfil público pode ser visitado.
- Perfil privado não expõe dados.

---

# Fase 5 — Segurança, Observabilidade e Qualidade

Objetivo: preparar a aplicação para uso real com proteção, logs e monitoramento básico.

## Parte 5.1 — Segurança HTTP e API

### Backend

- Configurar CORS por ambiente.
- Adicionar rate limiting.
- Adicionar headers de segurança.
- Revisar validação server-side.
- Garantir respostas de erro padronizadas.
- Adicionar proteção contra brute force no login quando aplicável.

### Frontend

- Tratar erros padronizados da API.
- Criar página de erro genérica.
- Criar Error Boundary.
- Garantir que rotas privadas não vazem conteúdo antes da validação de sessão.

### Testes

- Integração para CORS quando aplicável.
- Integração para rate limiting.
- Frontend: teste de Error Boundary.

### Documentação

- Documentar políticas de segurança.

### Critério de Aceite

- API possui proteções mínimas para exposição pública.
- Frontend trata falhas sem quebrar a aplicação.

## Parte 5.2 — Logging, CorrelationId e Health Checks

### Backend

- Configurar Serilog com logs estruturados.
- Adicionar CorrelationId por request.
- Incluir CorrelationId nas respostas de erro.
- Criar health checks para:
  - API
  - SQL Server
  - Redis
- Preparar estrutura para OpenTelemetry.

### Frontend

- Exibir mensagem amigável em erros inesperados.
- Enviar CorrelationId para suporte/debug quando disponível.

### Testes

- Integração para health checks.
- Teste garantindo presença de CorrelationId.

### Documentação

- Documentar como investigar erros por CorrelationId.
- Documentar endpoints de health check.

### Critério de Aceite

- Erros são rastreáveis por CorrelationId.
- Health checks indicam estado dos serviços.

## Parte 5.3 — Acessibilidade, Responsividade e UX

### Backend

- Garantir que endpoints retornem mensagens de validação adequadas para interface.

### Frontend

- Revisar páginas:
  - Home
  - Explorar cursos
  - Login
  - Cadastro
  - Dashboard
  - Minha lista
  - Criar curso
  - Editar curso
  - Perfil
  - Configurações
- Garantir:
  - Mobile first
  - Navegação por teclado
  - Labels acessíveis
  - Estados de loading
  - Empty states
  - Dark mode
  - Toast notifications

### Testes

- Testes de componentes principais.
- Testes de rotas principais.
- Validar acessibilidade básica com Testing Library.

### Documentação

- Documentar padrões de UI.

### Critério de Aceite

- Aplicação é utilizável em mobile e desktop.
- Fluxos principais possuem feedback visual consistente.

---

# Fase 6 — Sistema Social Futuro

Objetivo: adicionar base para seguir usuários e evoluir o feed.

## Parte 6.1 — Seguir e Deixar de Seguir Usuários

### Backend

- Criar entidade `UserFollow`.
- Criar commands:
  - `FollowUserCommand`
  - `UnfollowUserCommand`
- Criar queries:
  - `GetFollowersQuery`
  - `GetFollowingQuery`
- Criar endpoints:
  - `POST /api/users/{id}/follow`
  - `DELETE /api/users/{id}/follow`
  - `GET /api/users/{id}/followers`
  - `GET /api/users/{id}/following`

### Frontend

- Adicionar botão seguir/deixar de seguir no perfil público.
- Criar listas de seguidores e seguindo.
- Atualizar estado via React Query.

### Testes

- Unitários para regras de follow.
- Integração para seguir/deixar de seguir.
- Integração impedindo seguir a si mesmo.
- Frontend: teste de botão e contadores.

### Documentação

- Documentar endpoints sociais.

### Critério de Aceite

- Usuário consegue seguir outro usuário.
- Perfil mostra seguidores e seguindo.

## Parte 6.2 — Feed Personalizado

### Backend

- Criar query `GetPersonalizedFeedQuery`.
- Priorizar cursos públicos de usuários seguidos.
- Manter paginação.
- Criar endpoint `GET /api/feed/personalized`.

### Frontend

- Adicionar aba `Para você` no explorar.
- Exibir feed personalizado para usuários autenticados.
- Manter feed público para visitantes.

### Testes

- Integração para feed baseado em seguindo.
- Integração para paginação.
- Frontend: teste de alternância de abas.

### Documentação

- Documentar regra do feed personalizado.

### Critério de Aceite

- Usuário autenticado vê cursos de pessoas que segue em destaque.

---

# Fase 7 — Notificações, Recomendações e Analytics

Objetivo: evoluir a experiência após o MVP estar sólido.

## Parte 7.1 — Notificações

### Backend

- Criar entidade `Notification`.
- Gerar notificações para:
  - Curso compartilhado com usuário.
  - Novo seguidor.
  - Like recebido.
- Criar queries e commands:
  - `GetMyNotificationsQuery`
  - `MarkNotificationAsReadCommand`
- Criar endpoints:
  - `GET /api/notifications`
  - `PATCH /api/notifications/{id}/read`

### Frontend

- Criar ícone/central de notificações.
- Exibir contador de não lidas.
- Permitir marcar como lida.

### Testes

- Unitários para criação de notificações.
- Integração para listar notificações.
- Frontend: teste da central.

### Documentação

- Documentar eventos que geram notificação.

### Critério de Aceite

- Usuário recebe notificações relevantes dentro do app.

## Parte 7.2 — Recomendações Simples

### Backend

- Criar query `GetRecommendedCoursesQuery`.
- Base inicial:
  - Categorias mais usadas pelo usuário.
  - Cursos mais curtidos.
  - Cursos recentes públicos.
- Criar endpoint `GET /api/courses/recommended`.

### Frontend

- Criar seção `Recomendados para você`.
- Exibir cards no dashboard e explorar.

### Testes

- Unitários para query.
- Integração para recomendações.
- Frontend: teste da seção.

### Documentação

- Documentar algoritmo inicial.

### Critério de Aceite

- Usuário vê recomendações úteis sem depender de algoritmo complexo.

## Parte 7.3 — Analytics Básico

### Backend

- Criar eventos ou agregações para:
  - Visualizações de curso público.
  - Cliques no link de compra.
  - Likes recebidos.
- Criar query `GetMyCourseAnalyticsQuery`.
- Criar endpoint `GET /api/courses/{id}/analytics`.

### Frontend

- Criar painel simples de métricas no detalhe do curso do dono.
- Exibir visualizações, cliques e likes.

### Testes

- Integração para registrar visualização.
- Integração para registrar clique.
- Integração para consultar analytics apenas pelo dono.
- Frontend: teste do painel.

### Documentação

- Documentar eventos coletados.
- Documentar limitações de analytics inicial.

### Critério de Aceite

- Dono do curso consegue ver métricas básicas.

---

# Fase 8 — Preparação para Produto SaaS

Objetivo: preparar a base para monetização, escala e evolução futura.

## Parte 8.1 — Configurações e Preferências

### Backend

- Criar modelo de preferências do usuário.
- Criar endpoints para:
  - Tema preferido
  - Preferências de notificação
  - Privacidade padrão dos cursos

### Frontend

- Criar página `Configurações`.
- Permitir alterar tema, notificações e privacidade padrão.
- Persistir preferências.

### Testes

- Integração para salvar preferências.
- Frontend: teste da página de configurações.

### Documentação

- Documentar preferências suportadas.

### Critério de Aceite

- Usuário personaliza configurações básicas da conta.

## Parte 8.2 — Preparação para Assinatura Premium

### Backend

- Criar enum/plano inicial:
  - Free
  - Premium
- Adicionar claims/permissões para plano.
- Criar limites configuráveis:
  - Quantidade de cursos no plano Free.
  - Recursos futuros premium.
- Não integrar pagamento ainda; apenas preparar domínio e autorização.

### Frontend

- Criar componentes para exibir limites do plano.
- Exibir mensagens quando usuário atingir limite.
- Criar página placeholder de planos.

### Testes

- Unitários para regra de limite.
- Integração impedindo ultrapassar limite free.
- Frontend: teste de mensagem de limite.

### Documentação

- Registrar ADR sobre preparação para monetização.
- Documentar limites iniciais.

### Critério de Aceite

- Aplicação possui base para monetização sem acoplar pagamento prematuramente.

## Parte 8.3 — Preparação para Multi-Tenant Futuro

### Backend

- Avaliar pontos de extensão para tenant.
- Criar ADR definindo estratégia futura:
  - `TenantId` em entidades.
  - Separação lógica por coluna.
  - Possível evolução para bancos separados.
- Não adicionar complexidade se o MVP ainda não precisar.

### Frontend

- Não expor funcionalidade multi-tenant no MVP.
- Garantir que estrutura de sessão possa receber contexto futuro.

### Testes

- Architecture tests garantindo baixo acoplamento.

### Documentação

- Criar ADR de multi-tenancy futuro.

### Critério de Aceite

- Existe decisão documentada sem comprometer simplicidade do MVP.

---

# Ordem Recomendada de Implementação do MVP

1. Parte 0.1 — Estrutura do Repositório
2. Parte 0.2 — Docker e Ambiente Local
3. Parte 0.3 — Pipeline Base de Qualidade
4. Parte 1.1 — Registro de Usuário
5. Parte 1.2 — Login com JWT
6. Parte 1.3 — Refresh Token e Logout
7. Parte 2.1 — Modelo de Curso Desejado
8. Parte 2.2 — Criar Curso
9. Parte 2.3 — Minha Lista de Cursos
10. Parte 2.4 — Editar Curso
11. Parte 2.5 — Remover Curso
12. Parte 2.6 — Dashboard Inicial
13. Parte 3.1 — Tornar Curso Público ou Privado
14. Parte 3.2 — Feed Público
15. Parte 3.3 — Curtir e Descurtir Curso
16. Parte 3.4 — Compartilhamento por Link Público
17. Parte 3.5 — Compartilhamento Entre Usuários
18. Parte 4.1 — Busca de Cursos
19. Parte 4.2 — Perfil do Usuário
20. Parte 4.3 — Perfil Público
21. Parte 5.1 — Segurança HTTP e API
22. Parte 5.2 — Logging, CorrelationId e Health Checks
23. Parte 5.3 — Acessibilidade, Responsividade e UX

Ao concluir a ordem acima, o MVP estará funcional: usuários poderão se cadastrar, autenticar, criar cursos desejados, gerenciar sua lista, publicar cursos, compartilhar, curtir, buscar e manter perfil público.

---

# Checklist Padrão Para Cada Parte

Use este checklist antes de considerar qualquer parte finalizada:

- O domínio foi implementado sem depender de infraestrutura.
- A feature tem command/query separados.
- A feature tem validators.
- A API tem endpoint documentado no Swagger.
- O endpoint retorna `Result<T>` ou resposta padronizada equivalente.
- A migration foi criada quando houve alteração de banco.
- A listagem tem paginação quando retorna coleção.
- A autenticação/autorização foi aplicada quando necessário.
- O frontend tem página ou componente integrado.
- O frontend usa React Query para dados do servidor.
- O frontend usa Zustand apenas para sessão, auth ou tema.
- O formulário usa React Hook Form e Zod.
- Existem estados de loading, erro e vazio.
- Existem testes unitários relevantes.
- Existem testes de integração quando há banco, API ou autenticação.
- Existem testes frontend para fluxo principal.
- A documentação foi atualizada.
- O build local passa.
- O CI deve passar antes de avançar.

---

# Definition of Done do MVP

O MVP só deve ser considerado completo quando:

- Usuário cria conta, faz login, renova sessão e faz logout.
- Usuário recupera senha.
- Usuário cria, edita, remove e lista cursos desejados.
- Usuário controla visibilidade de cada curso.
- Visitantes visualizam cursos públicos.
- Usuários autenticados curtem cursos.
- Usuários compartilham cursos por link público.
- Usuários compartilham cursos com outros usuários.
- Busca funciona por nome, categoria e usuário.
- Perfil público respeita configuração de privacidade.
- API possui testes unitários, integração e arquitetura.
- Frontend possui testes dos fluxos principais.
- Docker executa ambiente completo.
- CI executa build, testes, lint e typecheck.
- Logs, health checks e CorrelationId estão ativos.
- UI é responsiva, acessível e possui dark mode.
- Documentação principal está atualizada.

---

# Backlog Pós-MVP

- Sistema social completo com seguidores.
- Feed personalizado.
- Notificações internas.
- Recomendações.
- Analytics de cursos.
- Preferências avançadas.
- Monetização premium.
- Integração futura com pagamentos.
- App mobile.
- Multi-tenant.
- Redis para cache de feed, busca e rankings.
- OpenTelemetry completo.
- Escalabilidade horizontal.
