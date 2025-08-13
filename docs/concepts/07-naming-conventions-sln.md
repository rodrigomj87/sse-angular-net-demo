# Naming Conventions (.sln & Projetos)

## 1. Objetivo
Estabelecer um padrão consistente de nomenclatura para solution, projetos .NET, namespaces, diretórios e frontend Angular garantindo clareza e escalabilidade.

## 2. Solution
Nome da solution: `SseDemo.sln`

## 3. Estrutura de Diretórios Backend
```
/ backend/
  SseDemo.sln
  /src/
    /SseDemo.Domain/
    /SseDemo.Application/ (opcional se crescermos)
    /SseDemo.Shared/ (contratos, DTOs cross-service)
    /SseDemo.OrdersService/
    /SseDemo.EventsService/ (futuro; agregador/event bus abstractions)
    /SseDemo.Gateway/ (expondo SSE + roteamento REST)
  /tests/
    /SseDemo.Domain.Tests/
    /SseDemo.OrdersService.Tests/
```

## 4. Nome de Projetos (.csproj)
- Domínio: `SseDemo.Domain`
- Shared: `SseDemo.Shared`
- Serviço de Pedidos: `SseDemo.OrdersService`
- Serviço de Eventos: `SseDemo.EventsService`
- Gateway/API: `SseDemo.Gateway`
- Tests: `<ProjectName>.Tests`

## 5. Namespaces
Seguem nome do projeto raiz: `SseDemo.Domain`, `SseDemo.OrdersService.Controllers`, etc.

## 6. Padrões de Pastas Internas
`Domain`:
```
/Entities
/ValueObjects
/Events
/Enums
/Abstractions
```
`OrdersService`:
```
/Controllers
/Services
/Repositories
/Mappers
/Configuration
```
`Gateway`:
```
/Endpoints (minimal APIs)
/Sse
/Middleware
/Infrastructure
```
`Shared`:
```
/Contracts
/DTOs
/Serialization
```

## 7. Sufixos e Prefixos
- Interfaces: prefixo `I` (ex: `IOrderRepository`)
- Eventos de domínio: sufixo `DomainEvent`
- DTOs: sufixo `Dto` ou Request/Response (ex: `CreateOrderRequest`)
- Services (aplicação): sufixo `Service`
- Handlers (se event sourcing crescer): sufixo `Handler`

## 8. Angular (frontend/)
Diretório: `/frontend/`
Projeto Angular: `sse-demo-ui`
Estrutura inicial:
```
/frontend/
  /src/app/
    core/ (services transversais ex: sse, api, interceptors)
    features/orders/ (componentes de pedidos)
    shared/ (componentes utilitários)
```

## 9. Ambiente e Config
- Arquivos de configuração: `appsettings.json` por projeto que necessite
- Variáveis sensíveis: `.env` (não commit) / `dotnet user-secrets` (se necessário)

## 10. Branches Git (detalhar task 1.8)
- `main` (estável)
- `dev` (integração)
- `feature/<slug>`

## 11. Scripts (futuro)
Scripts de automação em `/scripts` com nomes kebab-case: `run-all.ps1`, `build-backend.ps1`.

## 12. Convenções de Commits
Padrão Conventional Commits:
`feat:`, `fix:`, `chore:`, `docs:`, `test:`, `refactor:`, `perf:`

## 13. Justificativas
- Prefixo comum `SseDemo` facilita agrupamento no tooling
- Pastas claras evitam mistura de camadas
- Regras simples evitam bikeshedding

---
Este documento satisfaz a Task 1.7.
