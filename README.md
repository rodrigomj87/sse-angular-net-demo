# SSE Angular .NET Demo

Projeto demonstrativo integrando ASP.NET Core (.NET 8) e Angular usando Server-Sent Events (SSE) para atualização em tempo quase real de pedidos.

## Componentes
* backend/ (OrdersService + infra mínima)
* frontend/ (Angular workspace - orders-web)
* docs/ (conceitos, ADRs, glossário, tarefas)
* loadtest/ (scripts de teste de carga simples – se adicionados)

## Features Principais
* Endpoint SSE `/sse/stream`
* CRUD básico de pedidos + transições (`pay`, `fulfill`, `cancel`)
* Eventos SSE: `order-created` (payload flat) & `order-status-changed`
* Frontend sincroniza em tempo real (normaliza status uppercase, atualiza `updatedAt`)
* Health (`/health`) & readiness (`/ready`), logging estruturado com trace-id
* Métrica simples `/metrics/sse` (snapshot de conexões)
* Testes: unidade (domínio) + E2E SSE
* CI (GitHub Actions) build + test
* Script dev multi-processo com health check

## Executando
PowerShell (Windows):
```
./run-dev.ps1 -Restore
```
Flags úteis:
* `-NoFrontend` ou `-NoBackend`
* `-Port 5001` para mudar porta backend

Manual:
1. Backend: `dotnet run --project ./backend/src/SseDemo.OrdersService/SseDemo.OrdersService.csproj --urls http://localhost:5000`
2. Frontend: `cd frontend/orders-web && npm install && npm start`

## Teste Rápido SSE
Criar pedido:
```
curl -X POST http://localhost:5000/api/orders -H "Content-Type: application/json" -d '{"customerName":"Alice","totalAmount":123.45}'
```
Marcar como pago (substitua <id>):
```
curl -X POST http://localhost:5000/api/orders/<id>/pay
```
Observe atualização instantânea na UI sem refresh.

## Testes
Backend:
```
dotnet test backend/SseDemo.sln
```
Frontend (Angular):
```
cd frontend/orders-web
npm test
```

## Documentação
* Conceitos / ADRs / Glossário: `docs/`
* Mudanças: `CHANGELOG.md`
* Roadmap sugerido (futuro): auth, persistência, paginação, replay de eventos, métricas detalhadas.

## Licença
Projeto demonstrativo – ajuste conforme necessidade.
