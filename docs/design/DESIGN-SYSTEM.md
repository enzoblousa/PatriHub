# PatriHub — Sistema de Design

Registra a identidade visual do frontend (`frontend/patrihub-web`), decidida em conversa após
`01-SPEC-FUNCIONAL.md` §9 ter deixado isso propositalmente em aberto. Ponto de referência pra
qualquer tela nova ou revisão de tela existente — quando o CSS de um componente e este
documento discordarem, ajuste o CSS.

## 1. Direção

**Utilitário denso.** PatriHub não é um app consumer — é uma ferramenta que alguém abre todo
dia pra olhar números do próprio patrimônio. A referência é a mesa de operações (Bloomberg
terminal, dashboard de trading), não um app de banco pessoal: densidade de informação e
precisão numérica importam mais que calor visual ou espaço em branco generoso.

Três regras seguem disso, e valem mais que qualquer detalhe abaixo:

1. **Cor é sinal, nunca decoração.** Só duas cores carregam significado (§2) e só aparecem
   onde o significado existe. Nenhum elemento é colorido "pra ficar bonito".
2. **Número é mono, sempre.** Todo valor financeiro, percentual, data ou id usa
   `--font-mono` com `tabular-nums` — colunas de tabela alinham de verdade.
3. **Sem sombra, sem gradiente, sem ilustração.** Hierarquia vem de tipografia, espaçamento e
   hairlines (bordas de 1px), não de profundidade falsa.

## 2. Cor

Tema escuro (`:root` em `styles.css`) — grafite frio dominando, dois sinais semânticos:

| Token | Uso |
|---|---|
| `--bg` / `--surface` / `--surface-raised` | Fundo da página / cards e inputs / hover e elementos elevados |
| `--border` / `--border-strong` | Hairlines discretas / bordas de input, cabeçalho de tabela |
| `--text` / `--text-muted` / `--text-dim` | Texto principal / rótulos e legendas / cabeçalho de tabela, placeholder |
| `--sinal-pos` (+ `-dim`) | **Positivo/ativo/confirmação**: lucro ≥ 0, Contrato/Ativo em estado "bom", CTA primário (`button[type=submit]`), foco de input |
| `--sinal-neg` (+ `-dim`) | **Negativo/risco**: lucro < 0, erro, exclusão, zona de risco |

**Não existe um terceiro sinal** (ex.: âmbar pra "atenção"). Um status que não é claramente
bom nem ruim (`Vago`, `Manutenção`, `À venda`, `Encerrado`) fica neutro — ver §5.3. Resistir à
tentação de adicionar uma cor nova pra um caso específico; se parecer necessário, é sinal de
que o problema é outro (hierarquia tipográfica, agrupamento) — resolver por aí primeiro.

## 3. Tipografia

Uma família só, duas larguras — em vez de combinar duas famílias diferentes (o que a direção
anterior fazia): **IBM Plex Sans Condensed** pra UI, **IBM Plex Mono** pra todo número.
Reforça "sistema técnico" em vez de "produto de marca": os dois cortes existem na mesma
fundição, desenhados pra funcionar juntos.

- `--font-display` / `--font-body`: IBM Plex Sans Condensed. Títulos (`h1`/`h2`) e rótulos
  (`label`, `th`, `dt`, `button`) são **caixa alta com letter-spacing** — ver `styles.css`.
  Texto corrido normal (parágrafos, mensagens de erro) fica em caixa normal.
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
corpo em `--font-mono` (`td`). Hover de linha soma um
`border-left` de 2px em `--sinal-pos` — não é sinal de dado, é sinal de "isto é a linha sob o
cursor", então usa a mesma cor de foco/ação primária.

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

Minimalista, coerente com "sem decoração":

- `main` tem um fade-up de 250ms na entrada de cada rota (`@keyframes entrar`).
- O ponto antes de "PATRIHUB" no cabeçalho pulsa (`@keyframes pulsar`, 2.4s) — único elemento
  persistente animado do app, sinaliza "sessão viva numa mesa de dados".
- Ambos respeitam `prefers-reduced-motion: reduce` (`animation: none`).
- Sem hover-lift, sem transição de escala, sem confete. Se surgir vontade de adicionar uma
  animação nova, perguntar primeiro se ela carrega informação (estado mudou) ou é só
  enfeite — só a primeira se justifica aqui.

## 8. Textura

`body::before` aplica um scanline horizontal quase imperceptível (`oklch(100% 0 0 / 0.012)`,
listras de 1px a cada 3px) — textura de terminal sem virar ruído. É o único elemento
decorativo do sistema; não adicionar noise, grid ou padrão geométrico em cima disso.

## 9. Cobertura atual

Decisão desta rodada (ver conversa) foi aplicar o sistema a fundo em **3 telas-chave** antes
de estender — sistema fica validado antes de replicar:

| Tela | Profundidade |
|---|---|
| Login / Registro (`auth/`) | Completa — painel com rótulo de canto, borda superior de sinal |
| Dashboard (`dashboard/dashboard-pagina`) | Completa — readouts de estatística + tabela com sinal semântico |
| Ativos → Listagem (`ativos/ativos-lista`) | Completa — badge de status + sinal semântico de lucro |
| Todo o resto (Ativos detalhe/form, Lançamentos, Locatários, Contratos, Admin, Perfil) | Base global só — tokens, tipografia e componentes de `styles.css` já se aplicam; badge de status (§5.3) e sinal semântico de valor (§6) ainda não estendidos pras tabelas dessas telas |

**Próximo passo natural**: estender badge de status (§5.3) pro Status de Contrato
(`Ativo`/`Encerrado`/`Inadimplente` — `Ativo` = `--ok`) e sinal semântico de valor (§6) pra
`LancamentosLista` (Receita positivo / Despesa negativo, ou o valor em si) e
`AtivoDetalhe`. Cada extensão é o mesmo padrão já em `ativos-lista.ts`/`.html` — importar o
enum como valor (não `type`), expor no componente, bindar `[class.x]` no template.

## 10. Checklist pra uma tela nova

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
