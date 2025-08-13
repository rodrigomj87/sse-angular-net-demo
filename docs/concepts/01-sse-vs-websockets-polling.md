# Conceito de Server-Sent Events (SSE) e Comparação

## 1. O que é SSE?
Server-Sent Events (SSE) é um mecanismo padrão do HTML5 que permite que o servidor envie atualizações contínuas para o cliente através de uma conexão HTTP unidirecional mantida aberta. O cliente utiliza a interface `EventSource` (nativa em navegadores modernos) para receber eventos formatados de forma simples.

Características principais:
- Conexão HTTP única mantida aberta (streaming de texto)
- Unidirecional: servidor -> cliente
- Formato de evento simples (linhas iniciando com `data:`, `event:`, `id:`)
- Reconexão automática embutida no `EventSource`
- Suporte a `Last-Event-ID` para retomada (quando implementado pelo servidor)
- Texto UTF-8; dados estruturados (JSON) via serialização manual

## 2. Formato do Protocolo SSE
Cada evento é composto por linhas terminadas por `\n`. Exemplo:
```
event: order-created
data: {"id":"123","status":"NEW"}
id: 42

```
Linhas possíveis:
- `event:` (nome do tipo de evento, opcional; default = `message`)
- `data:` (pode repetir múltiplas linhas; concatenadas com `\n`)
- `id:` (identificador sequencial para retomada)
- `retry:` (sugere intervalo de reconexão em ms)

Um bloco vazio (linha em branco) sinaliza fim do evento.

## 3. Comparação SSE x WebSockets x Polling x Long Polling
| Aspecto | SSE | WebSockets | Polling | Long Polling |
|---------|-----|-----------|---------|--------------|
| Direção | Servidor -> Cliente | Bidirecional | Cliente -> Servidor (pull) | Servidor -> Cliente (simulado) |
| Complexidade | Baixa | Média/Alta | Muito baixa | Média |
| Overhead de Conexões | Baixo | Baixo | Alto (várias requisições) | Médio |
| Recurso Nativo Browser | `EventSource` | `WebSocket` | `fetch`/XHR | `fetch`/XHR |
| Ideal Para | Broadcast simples, notificações, streaming de updates | Chat, jogos, colaboração em tempo real | Atualizações raras | Quando SSE não é possível e precisa de near real-time |
| Reconexão Automática | Sim (built-in) | Não (manual) | N/A | Parcial (nova requisição) |
| Backpressure | Limitado (texto) | Suportado via framing | N/A | N/A |
| Binário | Não direto | Sim | Não | Não |
| Cache HTTP/Proxy Amigável | Sim (em geral) | Menos | Sim | Menos |
| Suporte HTTP/Infra Legada | Alto | Às vezes bloqueado | Alto | Alto |

Resumo: Use SSE quando só o servidor precisa empurrar eventos leves para o cliente de forma eficiente e simples. Use WebSockets quando há interação bidirecional ou requisitos de baixa latência constante. Use Polling apenas para simplicidade extrema ou baixa frequência. Long Polling é fallback quando SSE/WebSocket não são viáveis.

## 4. Casos de Uso Típicos de SSE
- Notificações de status (pedidos, fila de processamento)
- Atualizações de métricas ou dashboards
- Logs em tempo real (stream textual)
- Progresso de jobs de longa duração
- Eventos de domínio em sistemas reativos (somente saída)

## 5. Vantagens de SSE
- Simplicidade: sem handshake complexo
- Baseado em HTTP: atravessa proxies/firewalls mais facilmente
- Reconexão automática e event IDs
- Texto human-readable (facilita debug)
- Streaming natural (flush incremental)

## 6. Limitações de SSE
- Unidirecional (sem envio do cliente na mesma conexão)
- Apenas UTF-8 (para binário, precisa base64)
- Número de conexões pode ser limitado por navegadores (por host)
- Menos apropriado para latência ultra baixa interativa

## 7. Headers Importantes
Servidor deve enviar:
```
Content-Type: text/event-stream
Cache-Control: no-cache
Connection: keep-alive
```
E fazer flush após cada evento. Em ASP.NET Core, garantir `Response.Body.FlushAsync()` ou `await response.WriteAsync()` sem buffering.

## 8. Reconexão e Last-Event-ID
Quando a conexão cai, o navegador reconecta e envia header:
```
Last-Event-ID: <id>
```
Se o servidor suporta replay, pode reenviar eventos desde esse ID. Neste demo inicial: não haverá replay na F1/F2 (apenas fluxo ao vivo). Evolução futura: persistir eventos (ou offsets) e reconstituir.

## 9. Heartbeats
Para manter a conexão viva (e evitar timeouts intermediários), enviar periodicamente:
```
:data: \n
```
ou um comentário:
```
: ping\n\n
```
Comentários (`:`) são ignorados pelo cliente.

## 10. Sequenciamento e IDs
IDs crescentes simples (`id: 1`, `id: 2`, ...) bastam. Na horizontalização (múltiplas instâncias) é recomendável usar sequência global (ex: Redis INCR) ou timestamps ordenáveis (Snowflake-like) dependendo do caso.

## 11. SSE em Arquitetura de Microserviços
- Serviços produzem eventos de domínio internamente (InMemory event bus neste início)
- Um componente aggregator (Gateway ou Event Service) assina esses eventos e converte para SSE
- Evita cada serviço abrir seu endpoint SSE separado
- Permite padronização de payloads e controle central de conexões

## 12. Boas Práticas
- Formato consistente de payload: `{ type, timestamp, data }`
- Limitar tamanho de mensagens
- Fechar conexão ao detectar ocioso extremo ou abuso
- Monitorar quantidade de conexões ativas e taxa de eventos
- Implementar backoff de reconexão customizado no frontend, se necessário

## 13. Exemplo de Evento Padronizado (Projeto)
```
event: order-status-changed
data: {"type":"order-status-changed","timestamp":"2025-08-13T12:00:00Z","data":{"orderId":"a1","status":"PAID"}}
id: 17

```

## 14. Diferenças Práticas com WebSockets (Resumo Rápido)
| Tema | SSE | WebSocket |
|------|-----|-----------|
| Setup | Simples (HTTP GET) | Handshake upgrade para WS |
| Direção | Uni | Bi |
| Reenvio automático | Automático | Manual |
| Mensagens de controle | Comentários / heartbeats | Ping/Pong nativos |
| Binário | Não | Sim |
| APIs Browser | EventSource | WebSocket |

## 15. Quando NÃO usar SSE
- Necessidade de envio frequente cliente->servidor em canal persistente
- Transferência binária de alto volume
- Jogos, chat altamente interativo, colaboração colaborativa intensa

---
Este documento cobre a Task 1.1.
