# Backend (.NET) - Demo SSE

## Visão
Serviço ASP.NET Core responsável por:
- CRUD mínimo de Orders
- Emissão de eventos SSE em `/sse/stream`
- Métricas simples em `/metrics/sse`
- Health (`/health`) e Readiness (`/ready`)

## Estrutura
```
backend/
  src/SseDemo.OrdersService
    Controllers/
    Dtos/
    Domain/
    Sse/ (registry, broadcaster, pipeline helpers)
    Extensions/ (ServiceCollection + Middleware)
```

## Endpoints Principais
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | /api/orders | Lista pedidos |
| POST | /api/orders | Cria pedido |
| GET | /sse/stream | Abre canal SSE |
| GET | /metrics/sse | Retorna conexões ativas |
| GET | /health | Liveness |
| GET | /ready | Readiness agregada |

## Execução Local
Usar script raiz `./run-dev.ps1` (backend + frontend). Ou:
```
dotnet build backend/src/SseDemo.sln
cd backend/src/SseDemo.OrdersService
dotnet run
```
Acesse `http://localhost:5000/swagger` para documentação.

## Logging & Trace
Serilog configurado com correlação de trace-id. SSE inclui campos estruturados nos eventos.

## Escalabilidade (Futuro)
Redis Pub/Sub para fanout. Ver docs em `docs/observability`.

## Métricas
`/metrics/sse` expõe snapshot simples. Pode ser evoluído para Prometheus exporter.

## Próximos Passos Recomendados
- Autenticação / autorização
- Persistência real (db ou event store)
- Replay de eventos com Last-Event-ID
- Testes unitários e integração
