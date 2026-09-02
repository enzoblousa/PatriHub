# Recuperação de senha ("esqueci minha senha") sem sessão server-side

Faltava um jeito de um usuário recuperar acesso à conta quando esquecia a senha — o único reset
de senha que existia era o do Admin (`AdminService`, autenticado), sem nenhum fluxo anônimo. O
código já sinalizava essa lacuna: um comentário em `AdminService.cs` já citava "aquele fluxo
existe pro caso de 'esqueci minha senha' anônimo, com token expirável enviado por email" como algo
pendente.

**Escopo: só o self-service de "esqueci minha senha".** Troca de senha por um usuário já
autenticado (ex: tela de perfil) e reset de senha disparado pelo Admin (que já existe) ficam de
fora — são fluxos com telas e regras de autorização próprias, não haveria ganho em amarrar ao
mesmo ticket.

**Identificação só por email, fluxo de link.** `UserManager.GeneratePasswordResetTokenAsync`/
`ResetPasswordAsync` do próprio ASP.NET Core Identity já cobrem geração e validação de um token
opaco de uso único — não foi preciso desenhar um mecanismo de token do zero. O provider "Default"
(`DataProtectorTokenProvider`) não estava registrado (`AddIdentityCore` sozinho não habilita
nenhum provider); passou a ser via `.AddDefaultTokenProviders()`. O token expira em **30 minutos**
(`DataProtectionTokenProviderOptions.TokenLifespan`, configurado em `DependencyInjection.cs`) —
mesmo provider que seria usado por confirmação de email/2FA se um dia existirem, sem conflito de
janela porque não existem hoje.

**Decisão consciente de revelar quando o email não existe.** `SolicitarRecuperacaoSenhaAsync`
devolve 404 com "Não existe conta com este email." em vez da mensagem genérica de "se esse email
existir…" que costuma ser a recomendação padrão contra enumeração de contas. Trade-off aceito
deliberadamente em favor de UX mais direta — PatriHub é um SaaS pequeno, sem histórico de abuso
conhecido; revisitar se isso mudar.

**Invalidar sessões antigas sem servidor de sessão.** ADR-0001 já é stateless (JWT sem refresh
token, validade de ~7 dias) — não existe nada pra "derrubar" no reset além do próprio token, que
o cliente carrega sozinho. A solução: `ApplicationUser.SenhaAlteradaEm` grava quando a senha foi
trocada pela última vez; `JwtTokenGenerator` passou a incluir a claim `iat` explicitamente (não
dependia de nenhum default da lib antes); `SessaoInvalidadaMiddleware`, entre `UseAuthentication`
e `UseAuthorization` (`Program.cs`), rejeita com 401 qualquer requisição autenticada cujo `iat`
seja anterior a `SenhaAlteradaEm`. Um token sem claim `iat` (só possível num JWT emitido antes
desta mudança existir) é tratado como `DateTimeOffset.MinValue` — mais antigo que qualquer reset,
então é invalidado do mesmo jeito assim que o usuário troca a senha uma vez. `SenhaAlteradaEm` é
lido via `VerificadorSenhaAlterada`, com cache de 60 segundos por usuário (`IMemoryCache`) pra não
bater no Postgres em toda requisição autenticada — o efeito colateral é até 60s de janela pra um
token revogado realmente parar de funcionar, aceito pelo ganho de não consultar o banco toda hora.
O frontend não precisou de nenhuma mudança pra "fazer logout em todo dispositivo": o
`auth-interceptor.ts` já tratava 401 fazendo logout automático.

**Envio de email: Resend, sem lib de terceiro.** Não existia nenhuma integração de email no
projeto. Optamos por chamar a API REST do Resend direto via `HttpClient` (`ResendEnviadorDeEmail`)
em vez de um SDK — é uma chamada HTTP só, não justifica mais uma dependência (mesmo racional da
ADR-0007 evitando lib de máscara). Em dev/teste, sem `Resend:ApiKey` configurada, cai no fallback
`EnviadorDeEmailConsole`, que só loga o link em vez de mandar email de verdade — permite testar o
fluxo local e nos testes de integração sem precisar de conta no Resend. O remetente precisa estar
num domínio verificado no Resend (SPF/DKIM); a empresa registrou `patrihub.com.br` para isso — ver
README.md §Deploy para o estado do apontamento de DNS.

**UX**: página separada (`/esqueci-senha`, `/redefinir-senha`), não modal — mais simples de linkar
a partir do email e de testar. O link "Esqueci minha senha" aparece só na tela de login, não na de
cadastro. Sem CAPTCHA no endpoint de solicitação — só rate limiting por IP (mesma policy
`AuthEndpoints` de `login`/`registrar`), revisitar se houver abuso real. Depois de redefinir a
senha, o usuário é redirecionado pro login (sem login automático) — coerente com o próprio
mecanismo de invalidação de sessão descrito acima: não faria sentido logar automaticamente numa
sessão nova enquanto a mensagem é "todas as outras sessões foram encerradas".

**Validação de força de senha no frontend, só na tela nova.** `redefinir-senha.ts` valida a nova
senha no client (mínimo 8 caracteres, maiúscula, minúscula, dígito, caractere não-alfanumérico —
espelhando os defaults do ASP.NET Core Identity configurados em `DependencyInjection.cs`), com
mensagem por tipo de violação (`senha-validadores.ts`, seguindo o padrão da ADR-0008). A função
`mensagemErro` foi extraída de `ativos/ativos-validadores.ts` para `shared/formularios/
mensagem-erro.ts` pra ser reaproveitada aqui sem duplicar. **Fora de escopo**: retrofitar essa
mesma validação na tela de cadastro (`registro.ts`), que continua só com `required` — vira issue
separada se algum dia for feita, mesmo padrão que a própria ADR-0008 já tinha adotado ao deixar
Login/Registro de fora daquela entrega.
