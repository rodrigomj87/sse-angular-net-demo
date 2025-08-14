# SSE Angular .NET Demo

Projeto demonstrativo integrando ASP.NET Core (.NET 8) e Angular usando Server-Sent Events (SSE) para atualização em tempo quase real de pedidos.

## Componentes
* backend/ (OrdersService + infra mínima)
* frontend/ (Angular workspace - orders-web)
* docs/ (conceitos, ADRs, glossário, tarefas)
* loadtest/ (scripts de teste de carga simples – se adicionados)

## Features Principais
* Endpoint SSE `/sse/stream`
* CRUD básico de pedidos e publicação de eventos `order-created` / `order-status-changed`
* Health & readiness, logging estruturado com trace-id
* Testes unitários de domínio + teste E2E SSE
* CI (GitHub Actions) build + test

## Executando
```
./run-dev.ps1
```
Ou manual conforme detalhado em `docs/README.md`.

## Testes
```
dotnet test backend/SseDemo.sln
```

## Documentação
Ver `docs/` (conceitos, ADRs, glossário, tarefas). Mudanças: `CHANGELOG.md`.

## Licença
Projeto demonstrativo – ajuste conforme necessidade.
