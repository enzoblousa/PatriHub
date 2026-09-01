# Formulários usam máscaras próprias (sem lib), placeholder só em campo ambíguo, e obrigatoriedade sinalizada com asterisco + erro inline

Ao revisar os formulários de Ativo (Imóvel/Carro), constatamos que nenhum campo tem máscara,
poucos têm placeholder, e nenhum indica visualmente que é obrigatório — a única sinalização
existente é o submit ficar bloqueado, sem explicar por quê. Como isso tende a se repetir em
todo formulário futuro do PatriHub (Lançamentos, Contrato, etc.), registramos aqui a convenção
em vez de resolver caso a caso.

**Máscara de formatação, sem dependência nova.** O projeto não usa nenhuma lib de UI de
terceiros (nem Material, nem Bootstrap, nem `ngx-mask`) — todo CSS e toda lógica de formulário
são escritos à mão. Os formatos que precisamos (moeda BRL, percentual, placa de carro
brasileira, CEP, UF) são simples o bastante para não justificar trazer uma dependência só para
isso. Convenção: diretivas Angular próprias, agrupadas em `shared/mascaras/`, uma por formato,
reutilizáveis por qualquer formulário futuro.

**Placeholder só onde o formato não é óbvio.** Um placeholder em campo autoexplicativo (`nome`,
`marca`, `apelido`) só polui a tela e corre o risco de ser lido como valor já preenchido.
Convenção: placeholder é para comunicar *formato esperado* (`00000-000`, `AAA-0000` /
`AAA0A00`) ou desfazer uma ambiguidade real (ex: `matricula` de imóvel, cujo formato varia por
cartório) — nunca para dar um exemplo de conteúdo em campo já óbvio pelo próprio label.

**Obrigatoriedade: asterisco + legenda + validação de verdade.** Um asterisco em campo que não
bloqueia o submit de fato é pior do que nenhum indicador — cria falsa confiança. Convenção:
todo campo obrigatório leva `*` no label, o formulário leva uma legenda única
("* campos obrigatórios") no topo, e a obrigatoriedade sinalizada visualmente tem que
corresponder ao que a validação (idealmente espelhando as regras do backend/domínio) já impede
de submeter. Erro inline (classe `.campo-erro`, já existente em `styles.css`) e borda vermelha
aparecem só quando o campo foi tocado e está inválido (`touched && invalid`) — nunca no
formulário recém-aberto — e levam `aria-invalid`/`aria-describedby` ligando o campo à mensagem,
para leitor de tela.

**Campo condicional (ex: fieldset de Financiamento) segue a mesma regra, mas escopada:** um
campo só é marcado obrigatório enquanto a condição que o revela estiver ativa (`temFinanciamento()
=== true`); fora dela, fica sem asterisco e fora da validação.

Primeira aplicação: Imóvel e Carro (issues de implementação vinculadas a esta ADR). CEP com
autopreenchimento via serviço externo (ex: ViaCEP) e máscara de CPF no formulário de Locatário
ficam de fora por ora — são extensões razoáveis da mesma convenção, mas cada uma carrega escopo
próprio (chamada de rede externa, ou um formulário que esta revisão não tocou) que merece issue
separada.
