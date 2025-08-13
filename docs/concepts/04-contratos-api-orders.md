# Contratos de API REST - Orders

Base Path: `/api/orders`
Content-Type: `application/json`
Versão inicial: v1 (sem prefixo separado para simplicidade do demo)

## 1. Enum Status
```
CREATED | PAID | FULFILLED
```

## 2. DTOs
### 2.1 CreateOrderRequest
```
{
  "customerName": "string (1-100)",
  "totalAmount": number (>=0)
}
```
Validation:
- `customerName` obrigatório, trim, não vazio
- `totalAmount` obrigatório, >= 0

### 2.2 OrderResponse
```
{
  "id": "string (guid)",
  "customerName": "string",
  "totalAmount": number,
  "status": "CREATED|PAID|FULFILLED",
  "createdAt": "ISO-8601",
  "updatedAt": "ISO-8601"
}
```

### 2.3 ListOrdersResponse
```
{
  "items": [OrderResponse],
  "total": number
}
```
(Paginação simplificada omitida nesta fase; adicionar futuramente se necessário.)

### 2.4 UpdateStatusRequest (se implementarmos endpoint de atualização manual futura)
```
{
  "newStatus": "PAID|FULFILLED"
}
```

## 3. Endpoints
### 3.1 POST /api/orders
Cria um novo pedido.
Request Body: `CreateOrderRequest`
Responses:
- 201 Created + `OrderResponse`
- 400 Validation error

### 3.2 GET /api/orders
Lista todos os pedidos.
Query Params (futuro): `status`, `page`, `pageSize`
Responses:
- 200 OK + `ListOrdersResponse`

### 3.3 GET /api/orders/{id}
Obtém um pedido pelo id.
Responses:
- 200 OK + `OrderResponse`
- 404 Not Found

### 3.4 PATCH /api/orders/{id}/status (Futuro - não no MVP F1)
Altera status manualmente.
Request: `UpdateStatusRequest`
Responses:
- 200 OK + `OrderResponse`
- 400 invalid transition
- 404 not found
- 409 conflict (transição inválida)

## 4. Códigos de Erro Padrão
```
400 { code: "validation_error", errors: { field: "message" } }
404 { code: "not_found", message: "Order not found" }
409 { code: "invalid_status_transition", message: "..." }
500 { code: "internal_error", traceId: "..." }
```

## 5. Headers Relevantes
- `X-Trace-Id` (ecoar se enviado ou gerar)
- `Location` no 201 apontando para `/api/orders/{id}`

## 6. Contratos e Eventos (Relação)
A criação (`POST /api/orders`) dispara evento SSE `order-created`.
A mudança automática de status dispara `order-status-changed`.

## 7. Exemplos
### 7.1 Request Criação
```
POST /api/orders
{
  "customerName": "Alice",
  "totalAmount": 199.90
}
```
### 7.2 Response 201
```
{
  "id": "5f3b9c22-0f7b-4b31-b9ef-0c8d5e1e2a11",
  "customerName": "Alice",
  "totalAmount": 199.9,
  "status": "CREATED",
  "createdAt": "2025-08-13T12:00:00Z",
  "updatedAt": "2025-08-13T12:00:00Z"
}
```

## 8. Regras de Transição (Backend)
- CREATED -> PAID -> FULFILLED
- Qualquer outra sequência => 409

## 9. Considerações Futuras
- Paginação e filtros
- Query por intervalo de datas
- Campos adicionais: itens de linha, moeda, etc.
- Versionamento via header `Accept: application/vnd.demo.orders.v1+json`

---
Este documento satisfaz a Task 1.4.
