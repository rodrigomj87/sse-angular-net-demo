# Formato Health & Readiness

Endpoints implementados no Orders Service:

## /health (Liveness)
Objetivo: indicar se o processo está vivo. Não agrega checagens customizadas (apenas retorno 200 se o host responde).

## /ready (Readiness)
Resposta JSON agregando checagens registradas.

Exemplo de payload:
```json
{
  "status": "healthy",
  "checks": [
    { "name": "sse_registry", "status": "healthy", "data": { "activeClients": 0 } },
    { "name": "orders_repository", "status": "healthy", "data": { "sampleCount": 0 } }
  ]
}
```

Notas:
* `activeClients` indica conexões SSE ativas (métrica básica).
* `sampleCount` mostra quantos registros foram retornados na sonda mínima (limita custo).
* Futuras dependências externas (ex: Redis, DB real) devem ser adicionadas aqui via Health Checks.

Evoluções Futuras:
1. Adicionar tags (ex: readiness vs liveness) e filtragem por Predicate.
2. Expor métricas Prometheus com contadores de falhas por health check.
3. Diferenciar severidade (Degraded) quando número de clientes exceder threshold configurado.
