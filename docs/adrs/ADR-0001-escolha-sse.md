# ADR-0001: Escolha de Server-Sent Events (SSE) para Push Unidirecional

Data: 2025-08-13
Status: Accepted
Decisores: Arquitetura Demo
Revisores: (pendente)

## Contexto
Precisamos de mecanismo para entregar atualizações de status de pedidos em tempo quase real ao frontend. Requisitos:
- Simplicidade de implementação
- Baixo overhead
- Compatibilidade com proxies comuns
- Reenvio automático de conexão
- Somente servidor envia (sem necessidade de canal cliente->servidor persistente)

## Decisão
Adotar Server-Sent Events (SSE) como mecanismo de push unidirecional para o MVP e fases iniciais.

## Justificativa
- SSE lida nativamente com reconexão
- API de browser `EventSource` simples
- HTTP padrão, evita complexidade de upgrade/WebSocket
- Payload textual suficiente para nossos eventos (JSON leve)
- Fácil debug (visualizar no terminal ou browser)

## Alternativas Consideradas
1. WebSockets
   - Prós: bidirecional, binário, menor latência interativa
   - Contras: maior complexidade, reconexão manual, desnecessário para unidirecional
2. Long Polling
   - Prós: compatibilidade ampla
   - Contras: overhead de requisições, latência maior
3. Polling Intervalado
   - Prós: trivial
   - Contras: latência variável, requisições redundantes, escalabilidade pior

## Impactos
- Implementação SSE dedicada (endpoint stream)
- Necessidade de gerenciar conexões ativas
- Planejar escalabilidade (balanceador com suporte a conexões persistentes)

## Riscos
- Limite de conexões por navegador/host
- Falta de suporte Last-Event-ID no MVP inicial (perda de eventos em reconexão)

## Métricas de Sucesso
- Tempo médio de entrega < 1s
- Zero polling adicional para estados
- Estabilidade de conexões em testes locais

## Plano de Adoção
1. Implementar endpoint `/sse/stream`
2. Registrar clientes em registry
3. Broadcast de eventos de domínio
4. Heartbeat periódico

## Plano de Reversão
Migrar para WebSockets (substituir camada de transporte; manter EventBus)

## Referências
- WHATWG EventSource Living Standard
- MDN: Server-Sent Events
