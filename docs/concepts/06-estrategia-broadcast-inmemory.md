# Estratégia de Broadcast (In-Memory Initial Design)

## 1. Objetivo
Definir como eventos de domínio serão convertidos em mensagens SSE e distribuídos a todas as conexões ativas usando mecanismo in-memory simples na Fase 1/2.

## 2. Componentes
- `IEventBus` (pub/sub interno)
- `InMemoryEventBus` (lista de handlers Assíncronos)
- `ISseClientRegistry` (gestão de conexões SSE)
- `SseBroadcaster` (adapta eventos do domínio para formato SSE padronizado)
- `EventIdGenerator` (sequencial por processo)

## 3. Fluxo
1. Domínio publica um evento (ex: `OrderCreatedDomainEvent`) via `IEventBus.Publish(event)`
2. `InMemoryEventBus` itera handlers inscritos
3. Handler do `SseBroadcaster` recebe evento e mapeia para (eventName, payload JSON)
4. Gera ID sequencial e timestamp
5. Escreve linhas SSE para cada conexão ativa (stream) no `ISseClientRegistry`
6. Flush de cada resposta

## 4. Interfaces (esboço)
```csharp
public interface IEventBus {
    Task PublishAsync<T>(T @event, CancellationToken ct = default);
    void Subscribe<T>(Func<T, CancellationToken, Task> handler);
}

public interface ISseClientRegistry {
    Task AddAsync(string connectionId, SseClientConnection conn);
    Task RemoveAsync(string connectionId);
    IReadOnlyCollection<SseClientConnection> GetAll();
}

public sealed record SseClientConnection(string Id, HttpResponse Response, CancellationToken AbortToken);
```

## 5. Considerações de Concorrência
- Uso de `ConcurrentDictionary` em `ISseClientRegistry`
- Broadcast: snapshot (`ToArray()`) antes de iterar
- Escrita: cada resposta é independente; falhas removem conexão

## 6. Tratamento de Falhas
- Se `WriteAsync` ou `FlushAsync` lançar exceção => remover cliente
- Logar aviso com connectionId
- Não interromper broadcast dos demais

## 7. Backpressure / Performance
- Mensagens pequenas; risco baixo
- Futuro: fila interna por cliente se volume crescer
- Métrica: duração média de broadcast

## 8. Heartbeat
- Timer/background service a cada 15s chamando `BroadcastPing()`
- Envia evento `ping` igual aos demais (com `id:`)

## 9. Encerramento de Conexão
- Usar `HttpContext.RequestAborted` (AbortToken) para cancelar
- Remoção automática pelo registry ao detectar cancelamento

## 10. Evolução para Redis Pub/Sub
Substituir `InMemoryEventBus` por implementação:
- `RedisEventBus` assinando canais (`SUBSCRIBE domain-events`)
- Publish usando `PUBLISH`
- `SseBroadcaster` permanece igual (recebe eventos desserializados)

## 11. Mapeamento de Eventos de Domínio -> SSE
| Domínio | SSE event | data |
|---------|-----------|------|
| OrderCreatedDomainEvent | order-created | `{ orderId, status }` |
| OrderStatusChangedDomainEvent | order-status-changed | `{ orderId, oldStatus, newStatus }` |

## 12. Geração de ID
```csharp
private static long _seq; // Interlocked.Increment(ref _seq)
```
IDs reiniciam por processo no MVP; futuro: incluir prefixo de instância.

## 13. Segurança / Multi-Tenant (Futuro)
- Filtro por tenant no `SseBroadcaster`
- Registro de interesse do cliente (ex: somente pedidos do customer X)

## 14. Métricas
- Conexões ativas (`registry.Count`)
- Total de eventos enviados
- Erros de transmissão

## 15. Limitações do InMemory
- Não compartilha estado entre instâncias
- Não garante ordering global
- Perda total de eventos se processo reiniciar

---
Este documento satisfaz a Task 1.6.
