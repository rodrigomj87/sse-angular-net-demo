# Caso de Uso: Pedidos em Tempo Real

## 1. Objetivo
Demonstrar como eventos de domínio (mudança de status de pedidos) são propagados quase em tempo real para o frontend usando SSE, permitindo que a interface reflita atualizações sem polling.

## 2. Atores
- Usuário (operador interno ou cliente) que cria pedidos
- Sistema de Processamento (simulado) que altera estados de pedidos
- Frontend Angular consumindo eventos

## 3. Fluxo Principal
1. Usuário cria um novo pedido via POST `/api/orders` (status inicial: `CREATED`).
2. Backend persiste em repositório InMemory e publica evento `order-created`.
3. Serviço de eventos/Gateway publica via SSE para todos conectados.
4. Um job simulado processa o pedido e altera status para `PAID`, depois `FULFILLED`.
5. Cada transição gera eventos (`order-status-changed`).
6. Frontend atualiza lista e detalhe automaticamente.

## 4. Estados do Pedido
`CREATED` -> `PAID` -> `FULFILLED` (final)
Possível extensão futura: `CANCELLED`, `FAILED`.

## 5. Entidade (Modelo Simplificado)
```
Order {
  Id: string (GUID)
  CustomerName: string
  TotalAmount: decimal
  Status: OrderStatus (enum)
  CreatedAt: DateTimeOffset
  UpdatedAt: DateTimeOffset
}
```

## 6. Eventos de Domínio
- `OrderCreated { orderId, status, timestamp }`
- `OrderStatusChanged { orderId, oldStatus, newStatus, timestamp }`

Conversão para SSE (payload padrão):
```
event: order-created
data: {"type":"order-created","timestamp":"<ISO>","data":{"orderId":"...","status":"CREATED"}}

```
```
event: order-status-changed
data: {"type":"order-status-changed","timestamp":"<ISO>","data":{"orderId":"...","oldStatus":"CREATED","newStatus":"PAID"}}

```

## 7. APIs Envolvidas (Resumo – detalhar na Task 1.4)
- POST `/api/orders` cria pedido
- GET `/api/orders` lista pedidos
- GET `/api/orders/{id}` detalhe
- SSE `/sse/stream` stream de eventos

## 8. Regras de Negócio
- Pedido nasce como `CREATED`
- Job (simulado) altera para `PAID` após ~3s
- Outro passo altera para `FULFILLED` após ~3s adicionais
- Somente transições válidas são aceitas (não pular estados)

## 9. Non-Functional
- Latência alvo: < 1s entre mudança e entrega SSE local
- Conexões SSE mantidas com heartbeat a cada 15s
- Suporte a até algumas centenas de conexões simultâneas localmente

## 10. Erros / Edge Cases
- Criação sem campos obrigatórios -> 400
- Pedido não encontrado -> 404
- Atualização inválida de status (pular estado) -> 409

## 11. Extensões Futuras
- Persistência em banco (EF Core / SQLite)
- Replay com Last-Event-ID
- Filtragem de eventos por cliente (ex: somente pedidos do usuário)
- Autenticação + autorização por pedido

## 12. Métricas Desejadas
- Contador de pedidos criados
- Contador de transições de status
- Conexões SSE ativas

## 13. Motivadores da Escolha SSE Neste Caso
- Fluxo unidirecional (somente servidor envia eventos)
- Simplicidade de implementação
- Escalável horizontalmente com mínimo acoplamento

---
Este documento satisfaz a Task 1.2.
