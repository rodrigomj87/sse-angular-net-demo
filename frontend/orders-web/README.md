# OrdersWeb

Aplicação Angular que consome eventos SSE de pedidos em tempo real e interage com a API de Orders.

## Requisitos
- Node 20+
- Angular CLI instalada globalmente (opcional) ou usar via `npx`

## Desenvolvimento
```bash
npm install
npm start # ou: ng serve
```
Abre http://localhost:4200

## Configuração SSE
O serviço SSE conecta em `http://localhost:5000/sse/stream`.
Reconexão com backoff incremental (constantes definidas no serviço). Indicador visual mostra estado (online/offline/reconnecting).

## Criar Pedido
Formulário envia POST para `/api/orders`. Eventos de criação/status chegam via SSE e atualizam a lista.

## Estrutura Simplificada
```
src/app/
  services/orders-api.service.ts
  services/sse.service.ts
  components/orders-list/
  components/order-form/
```

## Testes
```bash
npm test
```
Inclui testes do wrapper SSE (reconexão, listeners, lifecycle).

## Build Produção
```bash
npm run build
```
Gera artefatos em `dist/` consumidos pelo Dockerfile (stage nginx).

## Docker
`docker-compose.yml` na raiz sobe backend + frontend (nginx) simultaneamente.

## Próximos Passos
- Externalizar configurações (URLs) via environment / file.
- Acrescentar paginação / filtros.
- Melhorar feedback de erros de rede.

## Referências
Ver documentação comparativa SSE em `docs/concepts/01-sse-vs-websockets-polling.md`.
