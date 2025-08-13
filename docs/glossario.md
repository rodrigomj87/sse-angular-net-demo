# Glossário

| Termo | Definição |
|-------|-----------|
| SSE | Server-Sent Events: streaming unidirecional texto via HTTP. |
| EventSource | API nativa do browser para consumir SSE. |
| Broadcast | Envio de um evento para todas as conexões ativas. |
| Heartbeat | Mensagem periódica para manter conexão viva. |
| Last-Event-ID | Header usado para retomada de sequência em reconexão. |
| In-Memory Bus | Implementação local de pub/sub em processo. |
| Fanout | Distribuição de um evento para múltiplos consumidores. |
| Backoff | Intervalo progressivo entre tentativas de reconexão. |
| Registry | Estrutura que mantém referência às conexões SSE ativas. |
| Trace Id | Identificador de correlação de requisições/eventos para logs. |
| Health Check | Verificação simples de disponibilidade básica do serviço. |
| Readiness | Verificação se dependências internas estão prontas. |
| Redis Pub/Sub | Mecanismo de publicação/assinatura distribuído. |
| Payload | Conteúdo do evento enviado ao cliente. |
| DTO | Data Transfer Object usado em APIs/Events. |
| Backpressure | Capacidade de limitar taxa de envio para não saturar consumidor. |
| Latência | Tempo entre geração do evento e recepção no frontend. |
| Idempotência | Garantia de não duplicar efeito ao reenviar evento/comando. |
