# 5.3 Estratégia de Escalabilidade Horizontal

## Objetivo
Permitir aumentar número de conexões SSE e throughput de eventos sem perda de consistência ou sobrecarga em um único nó.

## Fases
1. In-Memory (atual): um nó mantém conexões e emite eventos diretamente.
2. Pub/Sub (Redis): publishers enviam eventos para canal; cada nó consome e transmite aos seus clientes.
3. Sharding/Partition: segmentar clientes por chave (ex: hash de OrderId) distribuindo entre nós.
4. Fanout Service dedicado: serviço especializado em SSE/WebSocket assumindo responsabilidade de broadcast.

## Gargalos Atuais
- Broadcast O(N) síncrono no loop.
- Falta de backpressure.
- Reenvio redundante se múltiplos nós forem adicionados sem pub/sub.

## Evoluções Técnicas
| Fase | Ação | Benefício |
|------|------|-----------|
| 2 | Introduzir Redis pub/sub | Sincroniza eventos entre réplicas |
| 2 | Health + métricas agregadas | Balanceador pode usar para rota inteligente |
| 3 | Consistent hashing | Minimiza migração de conexões |
| 3 | Reconnect guidance header | Cliente pode reconectar direto ao shard correto |
| 4 | Fanout service + queue | Desacopla produção e entrega |

## Balanceamento
Use Load Balancer (round-robin) enquanto conexões são relativamente homogêneas. Evoluir para sticky session (cookie ou header) quando houver necessidade de manter afinidade.

## Redis Pub/Sub Sketch
```
Publisher -> Redis Channel "orders-events" -> Subscribers (N nós) -> SSE Broadcast
```
Cada nó mantém apenas seus clientes e assina o canal.

## Backpressure / Proteções Futuras
- Limite máximo de conexões por nó.
- Quota de eventos por intervalo.
- Compressão opcional (não padrão SSE) atrás de proxy.

## Plano Incremental
1. Adicionar camada Redis (5.4).
2. Introduzir métricas: eventos/seg, fila local (se implementada), latência broadcast.
3. Implementar sticky sessions se >1 nó.
4. Avaliar fanout dedicado se eventos/seg >> 1k.

## Riscos
- Ordem de entrega eventual diferente entre nós (OK para SSE eventual).
- Atraso de propagação Redis (tipicamente ms, aceitável).

## Decisão Atual
Seguir para Fase 2 (Redis Pub/Sub) antes de otimizações mais complexas.
