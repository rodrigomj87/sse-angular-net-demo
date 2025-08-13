# Troubleshooting SSE Demo

## Symptoms & Fixes
| Symptom | Likely Cause | Fix |
|---------|--------------|-----|
| SSE disconnects quickly | Proxy buffering | Ensure `X-Accel-Buffering: no` header and disable buffering in reverse proxy |
| No events received | CORS blocked | Check `Origin` matches allowed list (localhost:4200 / 3000) |
| Reconnect storm | Backend down or network offline | Verify backend health `/health`; frontend backoff should slow attempts |
| High CPU on server | Excessive broadcast loops | Introduce batching or offload to queue/Redis (future task) |
| 404 on refresh (frontend docker) | Nginx missing SPA fallback | Confirm modified `try_files` rule in Dockerfile |

## Quick Commands
```
# Run k6 smoke
k6 run loadtest/sse-smoke.js

# Curl stream (manual)
curl -N http://localhost:5000/sse/stream
```

## Logs
Serilog outputs structured logs with trace-id. Filter by `"eventType":"order-created"` for payload events.
