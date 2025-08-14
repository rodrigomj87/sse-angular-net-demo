# Changelog

Todas as mudanças notáveis deste projeto serão documentadas aqui.

O formato segue (inspirado em) Keep a Changelog e versionamento semântico.

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
- Ajuste em `Program` para `partial` visando testes de integração.

### Notes
- Persistência é in-memory (não persistente).
- Sem autenticação nesta versão.
- Próximas evoluções: Redis Pub/Sub, replay de eventos, auth.

[0.1.0]: https://github.com/rodrigomj87/sse-angular-net-demo/releases/tag/v0.1.0 (pendente criação)
