# 5.1 Estrutura de Logs

Os logs usam Serilog com correlação de trace-id. Abaixo exemplos de eventos principais.

## Request HTTP (middleware)
```json
{
  "Timestamp": "2025-08-13T22:50:00.123Z",
  "Level": "Information",
  "MessageTemplate": "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms",
  "Properties": {
    "TraceId": "c0a8014d4f6f2d15",
    "RequestMethod": "GET",
    "RequestPath": "/orders",
    "StatusCode": 200,
    "Elapsed": 12.34
  }
}
```

## Evento de Domínio -> Broadcast SSE
```json
{
  "Timestamp": "2025-08-13T22:50:02.500Z",
  "Level": "Information",
  "MessageTemplate": "Order created broadcast {OrderId}",
  "Properties": {
    "TraceId": "c0a8014d4f6f2d15",
    "OrderId": "1b3a0d5c-7d6e-4e7a-bf06-0c4f9c2b8f84",
    "EventType": "order-created"
  }
}
```

## Heartbeat SSE
```json
{
  "Timestamp": "2025-08-13T22:50:05.000Z",
  "Level": "Debug",
  "MessageTemplate": "SSE heartbeat broadcast ({ActiveClients} clients)",
  "Properties": {
    "TraceId": "c0a8014d4f6f2d15",
    "ActiveClients": 3
  }
}
```

## Erro no Broadcast
```json
{
  "Timestamp": "2025-08-13T22:50:10.000Z",
  "Level": "Warning",
  "MessageTemplate": "SSE client removed after send failure",
  "Properties": {
    "TraceId": "c0a8014d4f6f2d15",
    "ClientId": "c9c7d5c4-3f1a-48b2-9a6a-37e5b9d01b9d"
  }
}
```

## Campos Importantes
- TraceId: correlação ponta-a-ponta frontend -> backend -> SSE.
- EventType: tipo do evento SSE (order-created, order-status-changed, heartbeat).
- ActiveClients: métrica inline para heartbeat.

## Boas Práticas
1. Evitar logs em loop apertado.
2. Níveis: Information (negócio), Debug (diagnóstico), Warning (falhas recuperáveis), Error (falhas críticas).
3. Usar propriedades estruturadas sempre que possível.
