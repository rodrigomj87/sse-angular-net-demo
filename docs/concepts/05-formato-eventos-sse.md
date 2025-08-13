# Formato dos Eventos SSE (Padronização)

## 1. Objetivo
Definir um padrão consistente de serialização de eventos SSE para o projeto, facilitando evolução, parsing e depuração.

## 2. Estrutura Base
Camadas do evento SSE transmitido (linhas físicas):
```
event: <eventName>
id: <sequentialId>
data: <json>

```

Corpo JSON (`data:`) padronizado:
```
{
  "type": "<eventName>",
  "timestamp": "<ISO-8601 UTC>",
  "traceId": "<opcional>",
  "data": { ... payload do domínio ... },
  "meta": { "schemaVersion": 1 }
}
```
Campos:
- `type`: redundante em relação a `event:` porém facilita parsing caso concatenado
- `timestamp`: geração no momento de publicação
- `traceId`: propagado se existir (correlação logs)
- `data`: payload específico (ex: orderId, status, etc.)
- `meta.schemaVersion`: permite evolução de formato

## 3. Convenções de Nomes
- Kebab-case para `event:` (`order-created`, `order-status-changed`)
- `type` segue exatamente o valor de `event:`
- IDs sequenciais inteiros iniciando em 1 por instância (F1/F2)

## 4. Eventos Atuais
| event | Descrição | Payload `data` |
|-------|-----------|----------------|
| `order-created` | Novo pedido criado | `{ orderId, status }` |
| `order-status-changed` | Mudança de status | `{ orderId, oldStatus, newStatus }` |
| `ping` | Heartbeat | `{}` |
| `system-info` (futuro) | Info inicial na conexão | `{ version, instanceId }` |

## 5. Exemplos
### 5.1 order-created
Linhas SSE:
```
event: order-created
id: 7
data: {"type":"order-created","timestamp":"2025-08-13T12:00:00Z","traceId":"abc123","data":{"orderId":"f1","status":"CREATED"},"meta":{"schemaVersion":1}}

```
### 5.2 order-status-changed
```
event: order-status-changed
id: 8
data: {"type":"order-status-changed","timestamp":"2025-08-13T12:00:03Z","data":{"orderId":"f1","oldStatus":"CREATED","newStatus":"PAID"},"meta":{"schemaVersion":1}}

```
### 5.3 ping
```
event: ping
id: 15
data: {"type":"ping","timestamp":"2025-08-13T12:00:15Z","data":{},"meta":{"schemaVersion":1}}

```

## 6. Regras de Serialização
1. JSON compacto (sem espaços) para reduzir bytes
2. UTF-8 sem BOM
3. Escapar aspas internas no payload se necessário
4. Tamanho máximo recomendado por mensagem <= 8KB (soft limit)
5. `id:` sempre enviado (mesmo para `ping`) para facilitar métricas simples

## 7. Heartbeat
- Evento `ping` enviado a cada 15s (configurável)
- Cliente pode ignorar ou usar para sinalizar conexão ativa

## 8. Erros / Eventos de Sistema (Futuro)
Possível introdução de:
- `error` (não padrão SSE, porém como evento custom) com payload `{ code, message }`
- `disconnecting` antes de fechar quando possível

## 9. Evolução e Versionamento
- Aumento de `meta.schemaVersion`
- Novo campo não obrigatório => adição backward compatible
- Remoção de campo => requer major bump (ex: schemaVersion 2)

## 10. Last-Event-ID (Planejado Futura Fase)
- IDs monotônicos por instância não garantem ordenação global quando escalado
- Evolução: usar `instanceId:eventSeq` ou offset global (Redis INCR)

---
Este documento satisfaz a Task 1.5.
