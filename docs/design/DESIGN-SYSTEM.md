# PatriHub — Sistema de Design

Registra a identidade visual do frontend (`frontend/patrihub-web`), decidida em conversa após
`01-SPEC-FUNCIONAL.md` §9 ter deixado isso propositalmente em aberto. Ponto de referência pra
qualquer tela nova ou revisão de tela existente — quando o CSS de um componente e este
documento discordarem, ajuste o CSS.

*Histórico*: esta é a terceira direção visual do projeto. A primeira ("azul institucional")
foi descartada por ser genérica demais; a segunda ("utilitário denso", terminal escuro estilo
mesa de operações) foi descartada por ler como HUD de jogo em vez de ferramenta profissional.
Esta direção é inspirada em dashboards de fintech pessoal modernos (referências guardadas
fora do repo) — claro, arredondado, acolhedor — e foi adotada pro app inteiro de uma vez, não
só em telas-chave.

## 1. Direção

**Fintech pessoal amigável.** PatriHub acompanha o patrimônio de uma pessoa física — o tom é
de ferramenta financeira pessoal moderna (o tipo de app que alguém *gosta* de abrir pra ver
como o próprio dinheiro está indo), não um painel corporativo frio nem um terminal técnico.
Acolhimento e clareza vêm antes de densidade de informação.

Regras que valem mais que qualquer detalhe abaixo:

1. **Cartão, não grade.** Toda superfície de conteúdo (tabela, formulário, estatística) é um
   cartão com cantos arredondados e sombra leve — nunca uma grade crua encostada na borda da
   tela.
2. **Cor é sinal financeiro, não decoração geral.** Verde/vermelho são reservados pra valor
   com sinal (lucro, status "bom"/"ruim"); azul é a identidade da marca (ação primária, link,
   item ativo da navegação, foco). Nenhuma cor aparece só "pra ficar bonito".
3. **Caixa normal em tudo que se lê.** Nada de caixa alta em título, botão ou item de
   navegação — isso é herança da direção anterior (terminal) e não combina com o tom
   acolhedor desta.
4. **Uma família tipográfica, pesos fazem o trabalho.** Sem combinar mono + sans; hierarquia
   vem de peso e tamanho, não de trocar de fonte.

## 2. Cor

Tema único, claro (sem alternância claro/escuro — as referências desta direção são todas
claras, e inventar um escuro sem referência real não valia o esforço).

| Token | Uso |
|---|---|
| `--bg` / `--surface` / `--surface-muted` | Fundo da página (cinza-azulado bem claro) / cartões e inputs (branco) / hover, cabeçalho de tabela |
| `--border` / `--border-strong` | Borda padrão de cartão / borda de input e botão |
| `--text` / `--text-muted` / `--text-dim` | Texto principal / rótulos e legendas / texto terciário (placeholder, nota de rodapé) |
| `--accent` (+ `-hover`, `-soft`) / `--on-accent` | **Identidade da marca**: ação primária (`button[type=submit]`), link, item ativo da sidebar, anel de foco / texto sobre um fundo `--accent` sólido |
| `--sinal-pos` (+ `-soft`) | **Positivo/ativo**: lucro ≥ 0, Status `Alugado`/Contrato `Ativo` |
| `--sinal-neg` (+ `-soft`) | **Negativo/risco**: lucro < 0, erro, exclusão, zona de risco |

Sem terceiro sinal (ex.: âmbar). Um status que não é claramente bom nem ruim (`Vago`,
`Manutenção`, `À venda`, `Encerrado`) fica neutro (`--text-muted`/`--surface-muted`) — ver
§5.3.

**Nunca hardcode uma cor num `*.css` de componente** — sempre `var(--token)`. Se um tom novo
parecer necessário, é quase sempre sinal de que o problema é outro (hierarquia, agrupamento).

## 3. Tipografia

**Figtree** — única família do sistema, do título ao rótulo pequeno, variando peso (400–800),
não faixa larga (`--font-display` e `--font-body` apontam pro mesmo valor). Números usam
`font-variant-numeric: tabular-nums` dentro da própria Figtree (sem fonte mono separada) pra
coluna de tabela alinhar sem precisar trocar de tipo.

- `h1`/`h2`: peso 700, `letter-spacing: -0.01em` (aperta um pouco o título, comum em type
  scales modernos).
- Rótulo de campo (`label`), cabeçalho de tabela (`th`), termo de definição (`dt`): peso 600,
  tamanho pequeno, `--text-muted` — sem caixa alta (regra 3, §1).
- Número grande de destaque (readout de estatística, `.stat dd`): peso 700,
  `letter-spacing: -0.02em`, ~1.875rem.

## 4. Geometria, sombra e espaçamento

- `--radius-full: 999px` — botões, badges, chip de ação (`.acoes a`). Pílula é a forma padrão
  de controle interativo pequeno.
- `--radius-lg: 20px` — cartões grandes (tabela, painel de estatística, cartão de
  login/registro).
- `--radius: 12px` — inputs, botões secundários menores, cartão de tamanho médio.
- `--radius-sm: 8px` — reservado pra elemento pequeno que não deve ser pílula nem cartão
  grande (hoje sem uso ativo — existe pra não faltar um degrau menor quando precisar).
- **Sombra é elevação real agora** (ao contrário da direção anterior): `--shadow-sm` em
  elemento no nível da página (tabela, botão primário, badge não usa); `--shadow` em cartão
  "flutuante" isolado (login/registro, painel do Perfil). Nunca as duas ao mesmo tempo no
  mesmo elemento.
- Nenhum raio ou sombra fora desses tokens — inventar um valor pontual quebra a consistência
  entre telas.

## 5. Componentes

A maior parte disso vive em `styles.css` (global, sem CSS por componente) — ver o comentário
no topo do arquivo. CSS de feature (`*.css` de cada tela) é só ajuste de layout específico
daquela tela.

### 5.1 Botões

Pílula (`--radius-full`) sempre. `button[type=submit]` é a ação primária do form — preenchida
em `--accent`, com `--shadow-sm`; qualquer outro `button` é neutro (contorno, fundo branco).
`.excluir` é a única variante de risco — fundo `--sinal-neg-soft`, texto `--sinal-neg`. Não
criar uma variante nova sem necessidade real.

### 5.2 Tabelas

O `<table>` inteiro é o cartão — `border-radius: var(--radius-lg)` + `overflow: hidden` +
`box-shadow: var(--shadow-sm)` direto no elemento, sem precisar de um `<div>` wrapper (exige
`border-collapse: separate; border-spacing: 0` pro raio funcionar nas bordas das células).
Cabeçalho com fundo `--surface-muted`; hover de linha também `--surface-muted`, sem sinal de
cor (hover é neutro — só "isto está sob o cursor", ver regra 2, §1).

### 5.3 Badge de status

Pílula pequena com ponto colorido (`::before`) + texto, fundo `--surface-muted` por padrão.
Só a classe modificadora `--ok` (ex.: `badge-status--ok` em `AtivosLista`, pro Status
`Alugado`) usa `--sinal-pos`/`--sinal-pos-soft`. Ver `ativos-lista.css`/`.html` como
referência pra estender esse padrão a Contrato (`Ativo` = ok) e outros status.

### 5.4 Estatística em destaque

Um `dt`/`dd` de uma `<dl>` envolvidos num `<div class="stat">` (ver `dashboard-pagina.html`)
— cartão próprio (`--surface`, borda, `--radius-lg`, `--shadow-sm`) com o rótulo pequeno em
cima e o número grande embaixo. Envolver em `<div>` (em vez de deixar `dt`/`dd` soltos) é
válido em HTML5 dentro de `<dl>` e é o que permite cada estatística virar um cartão
independente.

### 5.5 Blocos de alerta

`.erro`, `.confirmacao`, `.aviso` e `[role="alert"]` (confirmação inline de exclusão) usam a
mesma anatomia — fundo `-soft` da cor do sinal, `border-radius: var(--radius)`, sem borda
lateral de destaque (isso seria o padrão "card com accent na lateral" que a diretriz de
design considera clichê — aqui a cor de fundo já basta). `.aviso` (informativo, sem sinal
pos/neg) usa o tom `--accent-soft`.

### 5.6 Formulário

Pilha vertical de `label` (peso 600, pequeno, `--text-muted`) + `input`/`select` com borda e
`--radius`, foco em anel azul suave (`box-shadow: 0 0 0 3px var(--accent-soft)`, sem outline
duro). `.filtro` (barra de filtro de uma listagem) é a exceção documentada: vira barra
horizontal via CSS de componente (`lancamentos-lista.css`).

## 6. Sinal semântico de valor

Duas classes utilitárias — `.valor-positivo` / `.valor-negativo` — coloram (e deixam em
negrito) um número quando o sinal dele tem significado direto pro usuário. Aplicadas via
`[class.valor-positivo]="x >= 0"` no template (ver `dashboard-pagina.html`,
`ativos-lista.html`), nunca automaticamente — nem todo número é lucro.

**Quando NÃO aplicar**: a coluna `Depreciação` do dashboard é o exemplo — negativo ali
significa valorização (bom), então colorir do mesmo jeito que Lucro inverteria a leitura sem
nenhuma legenda visível além da `<caption>` da tabela. Na dúvida se o sinal matemático
corresponde ao sinal semântico pro usuário, **não** aplique a classe.

## 7. Movimento

Mínimo, só pra suavizar transição de estado — nada em loop:

- `main` tem um fade-in de 200ms na entrada de cada rota (`@keyframes entrar`), respeitando
  `prefers-reduced-motion: reduce`.
- Botões e links de navegação têm transição curta de cor/fundo no hover (`0.12s`).
- Sidebar colapsa com transição de `width` (`0.16s`).
- Nada além disso — sem indicador piscando, sem confete, sem hover-lift decorativo.

## 8. Layout: cabeçalho + sidebar retrátil

`app.html`/`app.css` (`App`, o componente raiz) montam a casca da aplicação — nenhuma tela
individual reimplementa cabeçalho ou navegação.

- **Cabeçalho** (`.cabecalho`): barra branca de topo com borda inferior sutil. Botão de
  alternar a sidebar (só autenticado) + marca "PatriHub" à esquerda; nome do usuário + link
  Perfil + botão Sair à direita (só autenticado).
- **Sidebar** (`.sidebar`, dentro de `.corpo`): navegação vertical, só renderizada quando
  autenticado. Cada link é ícone (SVG inline, traço, sem preenchimento — nunca emoji) +
  `<span class="rotulo">`. `routerLinkActive="ativo"` marca o item da rota atual (pílula
  `--accent-soft` + texto `--accent`) — o link da raiz (`/`) usa
  `[routerLinkActiveOptions]="{ exact: true }"`.
- **Colapso**: `sidebarColapsada` (signal em `App`, persistido em
  `localStorage.patrihub.sidebarColapsada`) alterna `.sidebar--colapsada`, que estreita a
  largura e esconde `.rotulo` — só os ícones ficam.
- **Novo item de navegação**: siga o padrão de um `<a>` existente em `app.html` — ícone
  próprio, `routerLinkActive="ativo"`, `<span class="rotulo">`. Item só pra Admin vai dentro
  do mesmo `@if (auth.usuario()?.papel === 'Admin')` já usado pro link Admin.

## 9. Cobertura atual

Aplicado ao app inteiro nesta rodada — tokens globais (`styles.css`) cobrem toda tela sem CSS
próprio; um punhado de telas ganhou ajuste de layout específico:

| Tela/parte | O que tem CSS de componente |
|---|---|
| Casca (cabeçalho + sidebar, §8) | Layout completo da navegação |
| Login / Registro (`auth/`) | Cartão flutuante (`--shadow`, `--radius-lg`) |
| Dashboard (`dashboard/dashboard-pagina`) | `.stat` (cartão de estatística, §5.4) + `.taxa-referencia` horizontal |
| Ativos → Listagem (`ativos/ativos-lista`) | Badge de status (§5.3) |
| Perfil (`perfil/`) | `.dados-conta`/`.zona-de-risco` como cartão |
| Lançamentos → Listagem (`lancamentos/lancamentos-lista`) | `.filtro` horizontal + sinal semântico na coluna Valor (§6) |
| Ativo → Detalhe (`ativos/ativo-detalhe`) | Espaçamento de `.acoes-status` |
| Todo o resto (formulários de Ativo/Lançamento/Contrato/Locatário, Contratos/Locatários listagem, Admin) | Sem CSS próprio — herda 100% da base global (tabela-cartão, formulário, botão pílula) |

Sinal semântico de valor (§6) já cobre, além do Dashboard e `AtivosLista`, a coluna Valor de
`LancamentosLista` (Receita = positivo, Despesa = negativo — o sinal vem do `Tipo`, não do
número) e os espelhos de leitura do Admin (`admin-usuario-lancamentos`,
`admin-usuario-ativos`).

**Próximo passo natural**: estender badge de status (§5.3) pro Status de Contrato
(`Ativo`/`Encerrado`/`Inadimplente` — `Ativo` = `--ok`). `AtivoDetalhe` fica de fora do sinal
semântico de propósito — `AtivoDetalheDto` não expõe lucro, só `ValorAquisicao`/
`ValorMercadoAtual`, que não têm um sinal "bom/ruim" claro sem inventar uma comparação (mesma
cautela de §6 sobre Depreciação).

## 10. Checklist pra uma tela nova

1. Título com `<h1>`/`<h2>` — não precisa de classe, `styles.css` já cobre.
2. Tabela? `<table>` simples, sem `<div>` de wrapper — o cartão vem de graça do estilo global.
3. Estatística em destaque? Envolva `dt`/`dd` num `<div class="stat">` dentro da `<dl>` (§5.4).
4. Tem um valor com sinal significativo (lucro, saldo)? Aplique `.valor-positivo`/
   `.valor-negativo` via `[class.x]`, checando §6 antes.
5. Tem um status categórico? Considere um badge (§5.3) em vez de texto puro numa listagem.
6. Botão novo: `type="submit"` se for a ação primária do form, `type="button"` neutro caso
   contrário, `.excluir` só se for mesmo destrutivo.
7. Título, botão, link de navegação: caixa normal, nunca caixa alta (regra 3, §1).
8. Toda cor nova é `var(--token)`, nunca `oklch(...)`/hex direto num `*.css` de componente.
9. Rodou `ng build`? O budget de `anyComponentStyle` é 4-8kB por componente — se estourar, é
   sinal de que o CSS deveria estar em `styles.css`, não sendo duplicado ali.
