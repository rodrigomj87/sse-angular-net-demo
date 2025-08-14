# 5.4 Evolução para Redis Pub/Sub (Design)

## Objetivo
Desacoplar produção de eventos de sua distribuição para permitir múltiplos nós de broadcast SSE escaláveis horizontalmente.

## Topologia
```
(Order API / Domain) -> Publisher (IOrderEventPublisher) -> Redis (Channel: orders.events)
                                                     ^               |
                                                     |               v
                                             SSE Broadcaster (Subscriber N) -> Connected Clients
```

Cada instância do serviço Orders:
1. Publica o evento no canal Redis.
2. (Opcional) também processa localmente se o mesmo processo for assinante.

## Formato da Mensagem
JSON compacto (uma linha) para evitar parsing multiline:
```
{
  "eventType": "order-created",
  "id": "<guid>",
  "traceId": "<trace>",
  "payload": { ... campos já serializados hoje ... },
  "ts": "2025-08-13T22:55:00.123Z"
}
```

## Mudanças de Código (Previstas Fase 2)
- `IOrderEventPublisher` terá implementação `RedisOrderEventPublisher` usando `StackExchange.Redis`.
- Novo background service `RedisEventSubscriber` que:
  - Assina canal `orders.events`.
  - Faz deserialize seguro (try/catch + log Warning).
  - Converte em broadcast via `ISseClientRegistry.BroadcastAsync`.
- Feature flag (ex: `UseRedisPubSub`) para trocar implementação sem recompilar.

## Ordem e Idempotência
- Redis Pub/Sub não garante ordem global perfeita sob concorrência multi-produtor. Aceitável para update eventual.
- Idempotência pode ser feita usando campo `id` (OrderId) combinada com `eventType` se necessário (não implementado na fase inicial).

## Fault Tolerance
| Falha | Efeito | Mitigação |
|-------|--------|-----------|
| Redis indisponível | Eventos novos não entregues entre nós | Circuit breaker + fallback local broadcast temporário |
| Deserialização inválida | Evento descartado | Contador de falhas + log estruturado |
| Pico de eventos | Atrasos nos subscribers | Métrica de backlog (mensagens/s) + autoscale |

## Métricas Adicionais
- `redis_pubsub_messages_received_total`
- `redis_pubsub_deserialize_failures_total`
- `broadcast_latency_ms` (tempo entre publish e flush SSE) usando timestamp `ts`.

## Segurança
- Conexão Redis com AUTH + TLS (se disponível).
- Sem dados sensíveis no payload (somente campos já públicos para clientes SSE).

## Estratégia de Migração
1. Adicionar publisher Redis em paralelo (dual write: local broadcast + publish).
2. Implantar subscriber consumindo mas sem repassar (modo dry-run) para medir volume.
3. Ativar repasse (`EnableRedisFanout = true`).
4. Remover broadcast direto da implementação principal quando estável.

## Rollback
Desativar flag `EnableRedisFanout` mantendo dependências instaladas; não causa downtime.

## Próximos Passos
- Criar interfaces/implementações citadas.
- Adicionar configurações (`Redis:ConnectionString`).
- Acrescentar métricas e health check de conectividade Redis.

## Decisão
Seguir com Fase 2 usando Redis Pub/Sub com dual write inicial para validação.
