# Projeto Demo SSE (Server-Sent Events) - .NET + Angular + Microservices

Este diretório contém a documentação de planejamento, arquitetura e execução do projeto de demonstração de SSE.

## Objetivo
Construir um exemplo funcional simples, porém realista, demonstrando comunicação unidirecional em tempo real usando Server-Sent Events entre serviços .NET e um frontend Angular, dentro de uma arquitetura de micro-serviços.

## Componentes Planejados (Visão Macro Inicial)
- API Gateway (reverse proxy simples / YARP ou minimal) opcional
- Serviço de Emissão de Eventos (Event Producer Service)
- Serviço de Domínio (Orders / Pedidos) que gera eventos
- Canal SSE (endpoint que mantém conexão aberta)
- Frontend Angular que consome os eventos (EventSource)
- Serviço de Broadcast interno (in-memory + possibilidade de evolução para Redis Pub/Sub)
- Observabilidade mínima (logs estruturados + health checks)

## Fluxo Simplificado
1. Cliente Angular abre conexão SSE para `/sse/stream`.
2. Usuário cria/atualiza um pedido via API REST (`/api/orders`).
3. Serviço de Pedidos gera domínio -> publica evento interno.
4. Serviço de Emissão recebe evento e envia para todos os clientes conectados via SSE.
5. Frontend atualiza a UI em tempo quase real.

## Tecnologias
- .NET 8 (ASP.NET Core Minimal APIs ou Controllers onde fizer sentido)
- Angular 17+
- Docker / Docker Compose (para orquestração local opcional)
- YARP (se adotarmos gateway) ou direto (simplificação)
- Serilog para logging estruturado
- Redis (futuro / opcional) – inicialmente In-Memory

## Princípios
- Simplicidade primeiro, extensibilidade depois
- Separação de responsabilidades clara
- Documentar cada decisão arquitetural
- Mostrar diferenças SSE x WebSockets x Polling

## Estado Atual (v0.1.0 draft)
Versão inicial funcional pronta para tag `v0.1.0`:
* SSE funcionando (broadcast de criação e mudança de status de pedidos)
* Testes unitários e E2E mínimos verdes
* Observabilidade básica (logs estruturados, health, métricas simples)
* Documentação de conceitos e ADRs

## Como Executar
Backend + Frontend juntos:
```
./run-dev.ps1
```
Ou manual:
```
dotnet run --project backend/src/SseDemo.OrdersService
cd frontend/orders-web
npm install (primeira vez)
npm start
```
Acesse Swagger em `http://localhost:5000/swagger` e SSE no frontend em `http://localhost:4200` (quando implementado o componente de listagem).

## Testes
Executar todos os testes .NET:
```
dotnet test backend/SseDemo.sln
```
Teste E2E específico:
```
dotnet test backend/SseDemo.sln --filter FullyQualifiedName~SseSseFlowTests
```

## Próximas Evoluções
* Persistência real / Banco
* Redis Pub/Sub para escalabilidade horizontal
* Replay via Last-Event-ID
* Autenticação / autorização
* Métricas Prometheus detalhadas

---

Mantenha este README como porta de entrada. Atualize conforme decisões forem sendo tomadas.
