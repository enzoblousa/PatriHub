# PatriHub — Sistema de Design

Registra a identidade visual do frontend (`frontend/patrihub-web`), decidida em conversa após
`01-SPEC-FUNCIONAL.md` §9 ter deixado isso propositalmente em aberto. Ponto de referência pra
qualquer tela nova ou revisão de tela existente — quando o CSS de um componente e este
documento discordarem, ajuste o CSS.

## 1. Direção

**Utilitário denso, corporativo.** PatriHub não é um app consumer — é uma ferramenta que
alguém abre todo dia pra olhar números do próprio patrimônio. A referência é uma ferramenta
fintech B2B séria (o tipo de painel que um banco ou uma fintech dá pro próprio operador), não
um HUD de jogo nem um terminal de hacker: densidade de informação e precisão numérica
importam mais que calor visual, mas a execução tem que ler como profissional, nunca como
tela de menu ou console de estimação de terminal retrô.

Quatro regras seguem disso, e valem mais que qualquer detalhe abaixo:

1. **Cor é sinal, nunca decoração.** Só duas cores carregam significado (§2) e só aparecem
   onde o significado existe. Nenhum elemento é colorido "pra ficar bonito" — e mesmo essas
   duas cores são propositalmente contidas (chroma baixo), nunca neon.
2. **Número é mono, sempre.** Todo valor financeiro, percentual, data ou id usa
   `--font-mono` com `tabular-nums` — colunas de tabela alinham de verdade.
3. **Sem sombra, sem gradiente, sem ilustração.** Hierarquia vem de tipografia, espaçamento e
   hairlines (bordas de 1px), não de profundidade falsa.
4. **Caixa alta é rótulo estrutural, não título nem botão.** Reservada pra metadado pequeno
   (cabeçalho de tabela, rótulo de campo, badge) — ver §3. Título, botão e item de navegação
   ficam em caixa normal; caixa alta em tudo é o que lembra tela de menu de jogo.

*Nota de revisão*: a primeira versão desta direção usava verde/vermelho bem mais saturados,
scanline de fundo, um ponto pulsante no cabeçalho e caixa alta em título/botão/navegação —
lido como HUD de jogo, não como ferramenta profissional. Esta versão corrige isso; se uma
mudança futura reintroduzir qualquer um desses quatro elementos, é sinal de que a regra 4 (ou
o espírito geral desta seção) está sendo esquecida.

## 2. Cor

Escuro é o tema base (`:root` em `styles.css`) — grafite frio dominando, dois sinais
semânticos. Claro é a alternativa (`:root[data-theme='claro']`, ver §10), com os mesmos
tokens redefinidos — nunca cor direta fora desses dois blocos.

| Token | Uso |
|---|---|
| `--bg` / `--surface` / `--surface-raised` | Fundo da página / cards e inputs / hover e elementos elevados |
| `--border` / `--border-strong` | Hairlines discretas / bordas de input, cabeçalho de tabela |
| `--text` / `--text-muted` / `--text-dim` | Texto principal / rótulos e legendas / cabeçalho de tabela, placeholder |
| `--sinal-pos` (+ `-dim`, `-hover`) / `--on-sinal-pos` | **Positivo/ativo/confirmação**: lucro ≥ 0, Contrato/Ativo em estado "bom", CTA primário (`button[type=submit]`), foco de input / cor do texto sobre um fundo `--sinal-pos` sólido |
| `--sinal-neg` (+ `-dim`) | **Negativo/risco**: lucro < 0, erro, exclusão, zona de risco |

Ambos os tons de sinal usam chroma baixo (~0.09–0.13) de propósito — o suficiente pra ler como
verde/vermelho, longe do verde/vermelho saturado de HUD ou alerta de sistema.

**Não existe um terceiro sinal** (ex.: âmbar pra "atenção"). Um status que não é claramente
bom nem ruim (`Vago`, `Manutenção`, `À venda`, `Encerrado`) fica neutro — ver §5.3. Resistir à
tentação de adicionar uma cor nova pra um caso específico; se parecer necessário, é sinal de
que o problema é outro (hierarquia tipográfica, agrupamento) — resolver por aí primeiro.

**Nunca hardcode uma cor num `*.css` de componente** (nem em dark nem em light) — sempre
`var(--token)`. É o que permite o tema claro (§10) recolorir o app inteiro sem tocar em CSS
de feature; um `oklch(...)` direto ali quebra o toggle silenciosamente.

## 3. Tipografia

Uma família só, duas larguras — em vez de combinar duas famílias diferentes (o que a direção
anterior fazia): **IBM Plex Sans Condensed** pra UI, **IBM Plex Mono** pra todo número.
Reforça "sistema técnico" em vez de "produto de marca": os dois cortes existem na mesma
fundição, desenhados pra funcionar juntos.

- `--font-display` / `--font-body`: IBM Plex Sans Condensed. **Caixa alta com
  letter-spacing é só pra rótulo estrutural pequeno** — `label`, `th`, `dt`, `fieldset
  legend`, badge de status (§5.3) — ver `styles.css`. Título (`h1`/`h2`), `button`, link de
  navegação (`.navegacao a`, `.acoes a`) e a marca no cabeçalho ficam em **caixa normal**: são
  a coisa que se lê e se clica, não um metadado — caixa alta ali é o que lembra menu de jogo
  (regra 4, §1).
- `--font-mono`: IBM Plex Mono, com `font-variant-numeric: tabular-nums`. Todo `input`,
  `td`, `dd` usa mono por padrão (ver `styles.css`) — a exceção é uma célula de tabela com
  link/botão/form dentro (`td:has(a)`, `td:has(button)`, `td:has(form)`), que volta pra
  `--font-body` porque não é dado, é ação.

## 4. Geometria e espaçamento

- `--radius: 4px` (cards, painéis) / `--radius-sm: 2px` (inputs, botões) — cantos quase
  retos, não o `8px`/`12px` "app friendly" comum. Nunca usar um raio fora desses dois tokens.
- Sem `box-shadow` em lugar nenhum do sistema. Elevação e agrupamento vêm de
  `--surface`/`--surface-raised` + borda de 1px.
- Densidade: `font-size` de tabela é `0.8125rem` (menor que o corpo, `15px`) — tabela deve
  caber mais linha na tela, não ser confortável de ler frase por frase.

## 5. Componentes

A maior parte disso vive em `styles.css` (global, sem CSS por componente) — ver o comentário
no topo do arquivo. CSS de feature (`*.css` de cada tela) é só ajuste de layout específico
daquela tela.

### 5.1 Botões

`button[type=submit]` é a ação primária do form (preenchido, `--sinal-pos`); qualquer outro
`button` é neutro (contorno). `.excluir` é a única variante — contorno em `--sinal-neg`, pro
gatilho de uma exclusão (ver `AtivoDetalhe`, `Perfil`, `LancamentosLista`). Não criar uma
variante nova sem necessidade real — dois níveis (primário/neutro) mais um destrutivo bastam
pra tudo que o app faz hoje.

### 5.2 Tabelas

Componente central da direção. Cabeçalho em `--font-display` caixa alta pequena (`th`),
corpo em `--font-mono` (`td`). Hover de linha soma um `border-left` de 2px em
`--border-strong` — neutro de propósito: é só "isto é a linha sob o cursor", não um sinal de
dado, então não usa a cor de sinal (ver regra 1, §1).

### 5.3 Badge de status

Ponto colorido (`::before`) + rótulo mono caixa alta. Só a classe modificadora
`--ok` (ex.: `badge-status--ok` em `AtivosLista`, pro Status `Alugado`) usa `--sinal-pos`; sem
modificador, o badge fica neutro (`--text-dim`). Ver `ativos-lista.css`/`.html` como
referência pra estender esse padrão a Contrato (`Ativo` = ok) e outros status.

### 5.4 Blocos de alerta

`.erro`, `.confirmacao`, `.aviso` e `[role="alert"]` (confirmação inline de exclusão) usam a
mesma anatomia — fundo `-dim` da cor do sinal + `border-left` de 2px sólido, nunca borda
inteira ao redor. `.aviso` (informativo, sem sinal pos/neg) usa `--text-muted` no lugar de
uma cor de sinal.

### 5.5 Formulário

Pilha vertical de `label` (caixa alta, pequeno, `--text-muted`) + `input`/`select` em mono.
`.filtro` (barra de filtro de uma listagem) é a exceção documentada: vira barra horizontal
via CSS de componente (`lancamentos-lista.css`) em vez do empilhamento vertical padrão.

## 6. Sinal semântico de valor

Duas classes utilitárias — `.valor-positivo` / `.valor-negativo` — coloram um número quando
o sinal dele (positivo/negativo) tem significado direto pro usuário. Aplicadas via
`[class.valor-positivo]="x >= 0"` no template (ver `dashboard-pagina.html`,
`ativos-lista.html`), nunca automaticamente — nem todo número é lucro.

**Quando NÃO aplicar**: a coluna `Depreciação` do dashboard é o exemplo — negativo ali
significa valorização (bom), então colorir do mesmo jeito que Lucro (positivo=verde) inverteria
a leitura sem nenhuma legenda visível além da `<caption>` da tabela. Na dúvida se o sinal
matemático corresponde ao sinal semântico pro usuário, **não** aplique a classe — deixe o
número neutro.

## 7. Movimento

Minimalista ao ponto de quase não existir — nenhuma animação persistente/em loop (um elemento
pulsando ou piscando sem parar é HUD, não ferramenta profissional, ver regra 4 §1):

- `main` tem um fade-in de 200ms (opacidade só, sem deslocamento) na entrada de cada rota
  (`@keyframes entrar`), e respeita `prefers-reduced-motion: reduce` (`animation: none`).
- Fora isso, só transições de estado pontuais (`width` da sidebar ao colapsar, `border-color`
  num hover) — nunca decorativas, nunca em loop.
- Sem hover-lift, sem confete, sem indicador "ao vivo" piscando. Se surgir vontade de
  adicionar uma animação nova, perguntar primeiro se ela carrega informação (estado mudou) ou
  é só enfeite — só a primeira se justifica aqui, e mesmo assim uma vez, não em loop.

## 8. Layout: cabeçalho + sidebar retrátil

`app.html`/`app.css` (`App`, o componente raiz) são o único lugar que monta a casca da
aplicação — nenhuma tela individual reimplementa cabeçalho ou navegação.

- **Cabeçalho** (`.cabecalho`): barra fina de topo, sempre visível. Da esquerda pra direita —
  botão de alternar a sidebar (só quando autenticado), marca "PatriHub" (texto simples, caixa
  normal — ver §3), toggle de tema (§9, sempre visível, mesmo deslogado) e, só autenticado,
  nome do usuário + link Perfil + botão Sair.
- **Sidebar** (`.sidebar`, dentro de `.corpo`): navegação vertical, só renderizada quando
  autenticado. Cada link é ícone (SVG inline, traço `stroke-width: 1.5`, sem preenchimento —
  nunca emoji) + `<span class="rotulo">`. `routerLinkActive="ativo"` marca o item da rota
  atual (fundo `--sinal-pos-dim` + texto `--sinal-pos`) — o link da raiz (`/`) usa
  `[routerLinkActiveOptions]="{ exact: true }"` pra não ficar sempre ativo.
- **Colapso**: `sidebarColapsada` (signal em `App`, persistido em
  `localStorage.patrihub.sidebarColapsada`) alterna `.sidebar--colapsada`, que estreita a
  largura (`12rem` → `2.9rem`) e esconde `.rotulo` — só os ícones ficam, alinhados. Transição
  é só `width`, 160ms; nada de reflow brusco.
- **Novo item de navegação**: siga o padrão de um `<a>` existente em `app.html` — ícone
  próprio (não reaproveite o de outro item), `routerLinkActive="ativo"`, `<span
  class="rotulo">`. Se o item é só pra Admin, envolva no mesmo `@if
  (auth.usuario()?.papel === 'Admin')` já usado pro link Admin.

## 9. Tema claro/escuro

`core/tema/tema.ts` (serviço `Tema`) é a única fonte de verdade: um signal `tema` (`'escuro'
| 'claro'`), método `alternar()`, persistência em `localStorage.patrihub.tema`, aplicado como
`document.documentElement.setAttribute('data-theme', ...)`. `index.html` tem um script inline
que lê essa chave e já aplica `data-theme="claro"` antes do Angular subir — sem isso, uma
sessão em modo claro piscaria escuro por um frame no primeiro load.

- Escuro é o **padrão** (nenhum atributo, ou `data-theme="escuro"`); claro é
  `data-theme="claro"`. `:root[data-theme='claro']` em `styles.css` redefine todo token de
  cor de §2 — nunca parcialmente.
- O botão de toggle (`.botao-tema`, no cabeçalho) mostra o ícone do que **vai virar** ao
  clicar — sol enquanto está escuro (convite a clarear), lua enquanto está claro. Sempre
  visível, autenticado ou não.
- Testar uma mudança de cor nos dois temas antes de considerar pronta — o jeito mais rápido é
  `document.querySelector('.botao-tema').click()` no console do navegador.

## 10. Cobertura atual

Decisão desta rodada (ver conversa) foi aplicar o sistema a fundo em **3 telas-chave** antes
de estender — sistema fica validado antes de replicar:

| Tela/parte | Profundidade |
|---|---|
| Casca (cabeçalho + sidebar, §8) e tema claro/escuro (§9) | Completa, cross-cutting — todo o app, autenticado ou não |
| Login / Registro (`auth/`) | Completa — cartão simples, sem elemento decorativo |
| Dashboard (`dashboard/dashboard-pagina`) | Completa — readouts de estatística + tabela com sinal semântico |
| Ativos → Listagem (`ativos/ativos-lista`) | Completa — badge de status + sinal semântico de lucro |
| Todo o resto (Ativos detalhe/form, Lançamentos, Locatários, Contratos, Admin, Perfil) | Base global só — tokens, tipografia e componentes de `styles.css` já se aplicam; badge de status (§5.3) e sinal semântico de valor (§6) ainda não estendidos pras tabelas dessas telas |

**Próximo passo natural**: estender badge de status (§5.3) pro Status de Contrato
(`Ativo`/`Encerrado`/`Inadimplente` — `Ativo` = `--ok`) e sinal semântico de valor (§6) pra
`LancamentosLista` (Receita positivo / Despesa negativo, ou o valor em si) e
`AtivoDetalhe`. Cada extensão é o mesmo padrão já em `ativos-lista.ts`/`.html` — importar o
enum como valor (não `type`), expor no componente, bindar `[class.x]` no template.

## 11. Checklist pra uma tela nova

1. Título com `<h1>`/`<h2>` — não precisa de classe, `styles.css` já cobre.
2. Tabela? Estruture como as existentes (`<table>` simples, sem `<div>` de wrapper) — o
   resto vem de graça.
3. Tem um valor com sinal significativo (lucro, saldo)? Aplique `.valor-positivo`/
   `.valor-negativo` via `[class.x]`, checando §6 antes (o sinal matemático bate com o
   semântico?).
4. Tem um status categórico? Considere um badge (§5.3) em vez de texto puro, se a tela for
   uma listagem densa.
5. Botão novo: `type="submit"` se for a ação primária do form, `type="button"` neutro caso
   contrário, `.excluir` só se for mesmo destrutivo. Não crie uma variante de cor nova.
6. Rodou `ng build`? O budget de `anyComponentStyle` é 4-8kB por componente — se estourar,
   é sinal de que o CSS deveria estar em `styles.css`, não sendo duplicado ali.
7. Toda cor nova é `var(--token)`, nunca `oklch(...)`/hex direto num `*.css` de componente —
   ver §2. Testou a tela nos dois temas (`.botao-tema` no console, §9)?
8. Título, botão, link de navegação: caixa normal, nunca caixa alta (regra 4, §1 e §3).
