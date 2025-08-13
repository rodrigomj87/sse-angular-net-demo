# ADR-0002: Arquitetura Inicial In-Memory para Event Bus e Storage

Data: 2025-08-13
Status: Accepted
Decisores: Arquitetura Demo
Revisores: (pendente)

## Contexto
Na fase inicial precisamos provar fluxo ponta-a-ponta rápido sem dependências externas (Redis, Message Broker). Buscamos velocidade e simplicidade.

## Decisão
Utilizar implementações InMemory para:
- Event Bus (pub/sub simples)
- Repositório de Orders
- Geração de IDs sequenciais de eventos SSE

## Justificativa
- Permite foco no domínio e formato de eventos
- Elimina setup de infra extra (tempo de provisionamento)
- Fácil refatoração posterior substituindo interfaces

## Alternativas Consideradas
1. Redis Pub/Sub
   - Prós: escalável, persistência parcial (streams), multi-instância
   - Contras: overhead inicial, configuração necessária
2. RabbitMQ / Kafka
   - Prós: robustez, replays
   - Contras: complexo para MVP
3. Banco Relacional + triggers/polling
   - Prós: simplicidade de persistência
   - Contras: não push real-time nativo

## Impactos
- Não há replay de eventos
- Perda de dados em restart
- Sem ordenação global entre instâncias (monoprocesso inicial)

## Riscos
- Crescimento de complexidade ao migrar se interfaces não forem bem definidas

## Métricas de Sucesso
- Latência de broadcast baixa
- Facilidade de substituição por implementação Redis (≤ 1 dia de trabalho)

## Plano de Adoção
1. Definir interfaces (EventBus, Repository)
2. Implementar InMemory
3. Escrever testes básicos
4. Documentar pontos de troca

## Plano de Reversão
Migrar para Redis sem alterar clientes (apenas nova implementação registrada em DI)

## Referências
- Documentação .NET Concurrent Collections
