# Correlation & TraceId

Header padrão: `x-trace-id`.

Fluxo:
1. Middleware verifica se request possui `x-trace-id`.
2. Caso ausente gera GUID (`n` format) ou usa Activity.TraceId se disponível.
3. Valor é propagado em `Response` header e inserido em LogContext (Serilog) como `TraceId`.
4. SSE events incluem `traceId` no JSON para permitir correlação cliente -> logs.

Motivação:
* Depuração de fluxos assíncronos (REST -> SSE broadcast).
* Aderência futura a OpenTelemetry (Activity.TraceId já considerado).

Evoluções Futuras:
* Adicionar suporte a W3C Trace Context (`traceparent` / `tracestate`).
* Exportar spans via OTLP para observabilidade completa.
* Incluir `parentSpanId` em eventos para reconstrução de cadeia.
