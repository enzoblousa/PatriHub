# Frontend guarda o JWT em `localStorage`, não em cookie httpOnly

O access token JWT (vida longa, ~7 dias, sem refresh token — ver [ADR-0001](0001-sem-refresh-token-mvp.md))
é guardado em `localStorage` pelo Angular, e injetado manualmente no header `Authorization` de
toda request via um interceptor HTTP único. A alternativa mais segura seria um cookie
`httpOnly`/`Secure`/`SameSite=Strict`, que o navegador nunca expõe a JavaScript e portanto é
imune a roubo de token via XSS — mas exigiria o backend emitir o JWT como `Set-Cookie` além (ou
em vez) do corpo da resposta, e tratar CSRF (um cookie enviado automaticamente em toda request
cross-site precisa de proteção extra que o header `Authorization` manual dispensa). `localStorage`
é a opção mais simples — sem mudança no backend, sem CSRF a considerar — consistente com o
princípio "simples antes de completo" da Constituição.

**Consequência**: um token vazado por XSS fica válido por até 7 dias (sem revogação — mesma
janela de exposição já aceita pelo ADR-0001). Mitigado por não introduzir vetores de XSS no
frontend: Angular sanitiza bindings de template por padrão, então `innerHTML`/
`bypassSecurityTrust*` com dado vindo do usuário ou da API nunca deve ser usado, e o MVP não
carrega script de terceiro (sem analytics/ads/chat widget). Se o produto crescer a ponto de essa
janela de exposição não ser aceitável, revisitar junto com a introdução de refresh token
(ADR-0001) — as duas mudanças migrariam pra cookie `httpOnly` + rotation juntas.
