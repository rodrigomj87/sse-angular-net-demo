# Changelog

Todas as mudanças notáveis deste projeto serão documentadas aqui.

O formato segue (inspirado em) Keep a Changelog e versionamento semântico.

## [Unreleased]
### Added
- (placeholder)
### Changed
- (placeholder)
### Fixed
- (placeholder)
### Removed
- (placeholder)

## [0.2.0] - 2025-08-14
### Added
- Alinhamento do frontend (Angular) ao contrato backend: campos `customerName`, `totalAmount`, `updatedAt` em Orders.
- Normalização de status em tempo real via SSE (`order-created`, `order-status-changed`).
- Binding dinâmico de listeners SSE (adiciona listeners após conexão aberta sem perder eventos).
- De-dupe de eventos `order-created` e atualização de `updatedAt` em mudanças de status.
- Formulário de criação com validação e envio de `customerName` / `totalAmount`.
- Script `run-dev.ps1` aprimorado (porta configurável, health probe, PID management, flags `-NoFrontend` / `-NoBackend`).

### Changed
- Modelos frontend: remoção de `description`; inclusão de `customerName`, `totalAmount`, `updatedAt` e status em UPPERCASE.
- Serviço de API para consumir resposta paginada `{ items, total }` (ajuste de casing).
- `OrdersStore` refatorado para normalização de payload SSE flat.
- Removido pacote StyleCop.Analyzers temporariamente; categorias StyleCop e Documentation desativadas em `.editorconfig` para focar funcionalidade. (anotação histórica já aplicada antes)
- Ajustado `SseDemo.OrdersService.csproj` com `PreserveCompilationContext` e `AspNetCoreHostingModel=InProcess` para compatibilidade com testes E2E. (mantido no histórico consolidado)

### Fixed
- Eventos SSE não atualizavam status após mudança: corrigido mapeamento (`id` vs `orderId`, `newStatus` lower → upper).
- Lista duplicada de orders (duplicidade removida movendo `<app-orders-list>` para rota via `router-outlet`).
- Falha ao registrar listeners após conexão SSE inicial (agora listeners tardios são anexados imediatamente).
- Erro no teste E2E SSE (falha `testhost.deps.json`) resolvido ao usar `Program` correto do OrdersService em `WebApplicationFactory` (consolidado).

### Removed
- Uso antigo de `description` em modelos, componentes e chamadas HTTP.
- Arquivo duplicado `Class1.cs` em `SseDemo.Shared` (já removido previamente, consolidado aqui).

### Notes
- Próximos passos sugeridos: paginação real, replay de eventos, persistência (ex: Postgres/Redis), métricas Prometheus, autenticação.

## Migração 0.1.0 → 0.2.0
Nenhuma ação de migração necessária; apenas atualizar frontend. APIs existentes de criação e transição de status permanecem.

## [0.1.0] - 2025-08-13
### Added
- Backend .NET 8 (OrdersService) com SSE endpoint `/sse/stream`.
- Repositório in-memory de Orders e eventos `order-created`, `order-status-changed`.
- Middleware de correlação de trace + logging estruturado Serilog.
- Health (`/health`) e readiness (`/ready`).
- Métrica simples `/metrics/sse` (snapshot de conexões ativas).
- Frontend Angular base (serviço SSE resiliente, estrutura inicial).
- Documentação de conceitos (SSE vs WebSockets vs Polling, formato de eventos, ADRs).
- Testes unitários de domínio (OrderEntityTests).
- Teste E2E SSE (criação de pedido -> evento recebido) via WebApplicationFactory.
- GitHub Actions workflow (build + test).

### Changed
- Ajuste em `Program` para `partial` visando testes de integração (exposto para `WebApplicationFactory`).

### Notes
- Persistência é in-memory (não persistente).
- Sem autenticação nesta versão.
- Próximas evoluções: Redis Pub/Sub, replay de eventos, auth.

[0.2.0]: https://github.com/rodrigomj87/sse-angular-net-demo/releases/tag/v0.2.0
[0.1.0]: https://github.com/rodrigomj87/sse-angular-net-demo/releases/tag/v0.1.0
