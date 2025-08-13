# Documentação OpenAPI (Swagger)

Configurações principais:
* Documento: `v1` título "Orders Service API".
* Descrição inclui eventos SSE suportados.
* Operation Filter adiciona header opcional `x-trace-id`.
* Schema Filter fornece exemplo para `OrderResponse`.

Padrões de Resposta:
* Erros usam `ErrorResponse` com campos: code, message, errors (opcional).
* SSE não aparece como endpoint interativo (stream), mas `/sse/stream` descrito.

Headers relevantes:
* `x-trace-id`: correlação (aceito e retornado).
* `Content-Type: text/event-stream` para `/sse/stream`.

Evoluções Futuras:
1. Adicionar `ProblemDetails` mapping se migrarmos o contrato de erro.
2. Agrupar endpoints por tags adicionais (ex: StatusOps para pay/fulfill/cancel).
3. Gerar spec estática para distribuição (dotnet swagger tofile / CI pipeline).
