# Estratégia de Versionamento & Branches

## 1. Objetivo
Definir como o código evoluirá com controle de versões, nomenclatura de branches e tagging de releases neste demo.

## 2. Versionamento Semântico
Usaremos SemVer: `MAJOR.MINOR.PATCH`
- MAJOR: mudanças incompatíveis (ex: refatoração que quebra API pública)
- MINOR: novas features retrocompatíveis (ex: novo endpoint)
- PATCH: correções e melhorias internas sem alterar contratos

Primeira release planejada: `v0.1.0` (estado inicial funcional, não estável)

## 3. Branches Principais
- `main`: linha de release; sempre estável / testada
- `dev`: integração contínua de features; pode ficar levemente instável

## 4. Branches de Feature
Padrão: `feature/<slug-descritivo>`
Exemplos:
- `feature/orders-api`
- `feature/sse-endpoint`
- `feature/angular-ui`

Regras:
- Baseadas em `dev`
- Pull Request para `dev` (review leve)
- Rebase preferencial (limpa histórico) — merge commit apenas se necessário

## 5. Branches de Fix
`fix/<slug>` (bugs encontrados após merge em dev ou main)

## 6. Branches de Hotfix (se necessário após release)
`hotfix/<slug>` (base em `main`, merge em `main` e `dev`)

## 7. Tagging
Após merge em `main` e validação:
- Criar tag anotada: `git tag -a v0.1.0 -m "v0.1.0 initial demo"`
- Push: `git push origin v0.1.0`

## 8. Changelog (Simplificado)
Arquivo `CHANGELOG.md` (a criar próximo às releases):
Seções: Added / Changed / Fixed / Removed / Security

## 9. Commits
Conventional Commits + escopo opcional:
`feat(orders): criar endpoint POST`
`fix(sse): corrigir flush`
`docs: adicionar formato eventos`

## 10. Pull Requests
Checklist mínimo:
- Build passa
- Descrição clara
- Referência a task (ex: Task 2.5)
- Sem arquivos irrelevantes

## 11. Automatização (Futuro)
- GitHub Action: gerar release notes a partir de Conventional Commits
- Verificação lint/test em PR

## 12. Gestão de Versões Internas (Pacotes)
Se extraído pacote partilhado (ex: `SseDemo.Shared`), versionar via arquivo `Directory.Build.props` + CI (futuro).

## 13. Evolução de Schema SSE
Campo `meta.schemaVersion` pode divergir de versão SemVer da aplicação; manter tabela de compatibilidade futura.

## 14. Política de Deprecação
- Marcar endpoints obsoletos com atributo `[Obsolete]` (backend)
- Documentar remoções no CHANGELOG antes de major bump

---
Este documento satisfaz a Task 1.8.
