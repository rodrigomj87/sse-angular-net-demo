# Tarefas do Projeto Demo SSE (.NET + Angular)

Legenda de Status:
- [ ] Não iniciada
- [~] Em andamento
- [x] Concluída

## 1. Planejamento & Fundamentos
1.1 [x] Descrever conceito de SSE e comparação com WebSockets / Long Polling / Polling (ver `concepts/01-sse-vs-websockets-polling.md`)
1.2 [x] Definir caso de uso simplificado (Pedidos em tempo real) (ver `concepts/02-caso-uso-pedidos-tempo-real.md`)
1.3 [x] Definir diagrama de alto nível (serviços + fluxo) (ver `concepts/03-diagrama-alto-nivel.md`)
1.4 [x] Definir contratos de API REST (Orders) (ver `concepts/04-contratos-api-orders.md`)
1.5 [x] Definir formato dos eventos SSE (campos, event:, data:, id:) (ver `concepts/05-formato-eventos-sse.md`)
1.6 [x] Definir estratégia de broadcast (in-memory initial design) (ver `concepts/06-estrategia-broadcast-inmemory.md`)
1.7 [x] Definir naming conventions dos projetos (.sln structure) (ver `concepts/07-naming-conventions-sln.md`)
1.8 [x] Definir estratégia de versionamento / branches (ver `concepts/08-estrategia-versionamento-branches.md`)
1.9 [x] Criar ADRs base (template) (ver `adrs/ADR-TEMPLATE.md`, `adrs/ADR-0001-escolha-sse.md`, `adrs/ADR-0002-arquitetura-inmemory.md`)

## 2. Setup Backend (.NET)
2.1 [x] Criar solution e projetos: Core (Domínio), Orders.Service, Events.Service, Gateway (opcional), Shared
2.2 [x] Adicionar pacotes (Serilog, Swagger, HealthChecks)
2.3 [x] Implementar modelo Order + eventos de domínio
2.4 [x] Implementar repositório InMemory de Orders
2.5 [x] Implementar API REST de Orders (CRUD mínimo + criar)
2.6 [x] Implementar serviço SSE (endpoint /sse/stream) (registry, endpoint, publisher, heartbeat OK)
2.7 [x] Implementar hub interno de conexão (SseClientRegistry)
2.8 [x] Integrar Orders -> publicar evento -> SSE broadcast (order-created + status-changed)
2.9 [x] Health checks (/health) e /ready (custom sse_registry + repository + JSON readiness response)
2.10 [x] Logging estruturado + correlação (trace-id middleware + enrichment + SSE payload)
2.11 [x] Documentação Swagger/OpenAPI (trace header filter, exemplos OrderResponse & ErrorResponse, descrição endpoint SSE)

## 3. Frontend (Angular)
3.1 [x] Criar projeto Angular base
3.2 [x] Serviço Angular para SSE (EventSource wrapper + reconexão)
3.3 [x] Serviço Orders API (HttpClient)
3.4 [x] Componente de listagem de pedidos em tempo real
3.5 [x] Form para criar novo pedido
3.6 [x] Indicação visual de conexão (online/offline SSE)
3.7 [x] Tratamento de reconexão e backoff (configurável + watchdog + offline handling)
3.8 [x] Pequenos testes unitários (serviço SSE) (backoff, listeners, watchdog, offline/online, controls, destroy)

## 4. Integração & Execução
4.1 [x] Script de execução simultânea (backend + frontend)
4.2 [x] Dockerfile(s) backend
4.3 [x] Dockerfile frontend
4.4 [x] docker-compose.yml (rede interna + variáveis)
4.5 [x] Ajustar CORS e headers corretos para SSE
4.6 [x] Teste de carga leve (k6 ou autocannon) opcional
4.7 [x] Guia de troubleshooting

## 5. Observabilidade & Robustez
5.1 [x] Estrutura de logs (exemplos) na doc
5.2 [x] Métrica simples (contador de conexões ativas)
5.3 [x] Estratégia de escalabilidade horizontal (documentação)
5.4 [x] Evolução para Redis Pub/Sub (design doc)

## 6. Documentação Complementar
6.1 [x] ADR-0001 - Escolha de SSE (revisada e finalizada)
6.2 [x] ADR-0002 - Arquitetura inicial in-memory (revisada e finalizada)
6.3 [x] Comparativo SSE x WebSockets x Polling (ver `concepts/01-sse-vs-websockets-polling.md`)
6.4 [x] FAQ (ver `faq.md`)
6.5 [x] Glossário (ver `glossario.md`)
6.6 [x] README frontend (ver `../frontend/orders-web/README.md`)
6.7 [x] README backend (ver `../backend/README.md`)

## 7. Qualidade & Automação
7.1 [x] Configurar lint + analyzers .NET (Directory.Build.props + analyzers)
7.2 [x] Configurar ESLint + Prettier Angular (já presente .eslintrc.cjs + scripts lint/format)
7.3 [x] GitHub Actions (build + test) opcional (workflow ci.yml)
7.4 [x] Testes unitários backend (mínimos) (OrderEntityTests)
7.5 [ ] Testes e2e simples (criar pedido -> receber evento)

## 8. Finalização
8.1 [ ] Revisão geral de código
8.2 [ ] Atualizar docs com passos finais
8.3 [ ] Tag v0.1.0

---

## Próxima Task Sugerida
Preparar seção 6 (Documentação complementar) iniciando ADR reviews
