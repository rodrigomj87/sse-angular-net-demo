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
2.1 [ ] Criar solution e projetos: Core (Domínio), Orders.Service, Events.Service, Gateway (opcional), Shared
2.2 [ ] Adicionar pacotes (Serilog, Swagger, HealthChecks)
2.3 [ ] Implementar modelo Order + eventos de domínio
2.4 [ ] Implementar repositório InMemory de Orders
2.5 [ ] Implementar API REST de Orders (CRUD mínimo + criar)
2.6 [ ] Implementar serviço SSE (endpoint /sse/stream)
2.7 [ ] Implementar hub interno de conexão (SseClientRegistry)
2.8 [ ] Integrar Orders -> publicar evento -> SSE broadcast
2.9 [ ] Health checks (/health) e /ready
2.10 [ ] Logging estruturado + correlação (trace-id)
2.11 [ ] Documentação Swagger/OpenAPI

## 3. Frontend (Angular)
3.1 [ ] Criar projeto Angular base
3.2 [ ] Serviço Angular para SSE (EventSource wrapper + reconexão)
3.3 [ ] Serviço Orders API (HttpClient)
3.4 [ ] Componente de listagem de pedidos em tempo real
3.5 [ ] Form para criar novo pedido
3.6 [ ] Indicação visual de conexão (online/offline SSE)
3.7 [ ] Tratamento de reconexão e backoff
3.8 [ ] Pequenos testes unitários (serviço SSE)

## 4. Integração & Execução
4.1 [ ] Script de execução simultânea (backend + frontend)
4.2 [ ] Dockerfile(s) backend
4.3 [ ] Dockerfile frontend
4.4 [ ] docker-compose.yml (rede interna + variáveis)
4.5 [ ] Ajustar CORS e headers corretos para SSE
4.6 [ ] Teste de carga leve (k6 ou autocannon) opcional
4.7 [ ] Guia de troubleshooting

## 5. Observabilidade & Robustez
5.1 [ ] Estrutura de logs (exemplos) na doc
5.2 [ ] Métrica simples (contador de conexões ativas)
5.3 [ ] Estratégia de escalabilidade horizontal (documentação)
5.4 [ ] Evolução para Redis Pub/Sub (design doc)

## 6. Documentação Complementar
6.1 [ ] ADR-0001 - Escolha de SSE
6.2 [ ] ADR-0002 - Arquitetura inicial in-memory
6.3 [ ] Comparativo SSE x WebSockets x Polling
6.4 [ ] FAQ
6.5 [ ] Glossário
6.6 [ ] README frontend
6.7 [ ] README backend

## 7. Qualidade & Automação
7.1 [ ] Configurar lint + analyzers .NET
7.2 [ ] Configurar ESLint + Prettier Angular
7.3 [ ] GitHub Actions (build + test) opcional
7.4 [ ] Testes unitários backend (mínimos)
7.5 [ ] Testes e2e simples (criar pedido -> receber evento)

## 8. Finalização
8.1 [ ] Revisão geral de código
8.2 [ ] Atualizar docs com passos finais
8.3 [ ] Tag v0.1.0

---

## Próxima Task Sugerida
Iniciar pela 2.1: Criar solution e projetos.
