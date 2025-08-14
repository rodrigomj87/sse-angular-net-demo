# FAQ - Projeto Demo SSE

## 1. Por que SSE e não WebSockets?
Porque o caso de uso é unidirecional (servidor -> cliente). SSE reduz complexidade (reconexão automática, HTTP puro) e atende requisitos de latência e simplicidade.

## 2. Como escalar horizontalmente?
Adicionar Redis Pub/Sub para fanout e usar balanceador com stickiness ou mover canal SSE para um nó dedicado. Ver `observability/escalabilidade-horizontal.md` e `observability/redis-pubsub-design.md`.

## 3. Como saber quantas conexões estão ativas?
Endpoint `GET /metrics/sse` retorna `{ activeConnections, capturedAt }`.

## 4. Há replay de eventos após reconexão?
Não na versão atual. O fluxo é somente "live". Evolução: armazenar eventos (Redis Streams ou persistência) e usar `Last-Event-ID`.

## 5. O que acontece se o backend reiniciar?
Todas conexões caem e o frontend reconecta. Eventos emitidos durante a janela de reinício são perdidos (sem replay).

## 6. Como depurar SSE no navegador?
Aba Network > request `/sse/stream` > visualizar "EventStream". Também é possível usar `curl -N http://localhost:5000/sse/stream`.

## 7. Como customizar backoff no frontend?
O wrapper Angular implementa reconexão incremental configurável. Ajustar constantes no serviço SSE (futuro: externalizar para config).

## 8. Por que não usar gRPC streaming?
Para evitar dependência binária adicional e manter simplicidade HTTP padrão demonstrativa; browsers consomem SSE nativamente.

## 9. Qual formato de evento?
`event: <tipo>` + `data: { type, timestamp, data }` + `id: <seq>`.

## 10. Como adicionar um novo tipo de evento?
Publicar domínio -> mapear para payload SSE padronizado -> emitir via broadcaster (adicionar constantes de nome e DTO se necessário).

## 11. Latência esperada?
Local: <1s (quase instantâneo). Rede real depende de jitter e flush.

## 12. Estratégia de heartbeat?
Comentários `: ping` ou eventos vazios a cada N segundos para manter conexões ativas.

## 13. Limites conhecidos?
Sem binário nativo, sem bidirecionalidade, limite de conexões por host (browser). Para muitos clientes simultâneos considerar redistribuição / Redis.

## 14. Observabilidade mínima inclusa?
Sim: logs estruturados, trace correlation, health/readiness, métrica de conexões ativas.

## 15. Próximos passos recomendados?
Adicionar replay, autenticação, dashboards de métricas e CI/CD.
