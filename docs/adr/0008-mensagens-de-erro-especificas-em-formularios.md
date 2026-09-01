# Mensagem de erro por tipo de violação, com o formato subindo pro backend quando faltava regra

A ADR-0007 deu obrigatoriedade visível (asterisco + legenda) e erro inline aos formulários de
Ativo, mas cada campo mostrava **uma** mensagem fixa, não importa qual validador tivesse falhado.
Um campo com `required` + `min(0)` (ex: `valorAquisicao`) mostrava sempre "Informe um valor de
aquisição válido." — não dizia se o problema era campo vazio ou valor inválido. Pior: `placa`,
`cep` e `uf` tinham máscara de formatação (`appPlaca`, `appCep`) mas **nenhum validador de
formato** — a máscara só deixa bonito o que foi digitado, não impede submeter "AA" como placa ou
"123" como CEP. Investigando a fonte da verdade (`Carro.cs`, `Endereco.cs`), o backend também não
tinha regra de formato pra esses campos — só "não vazio" (e `Uf` só checava `Length == 2`, sem
validar contra as 27 UFs reais). Não tinha o que o frontend "espelhar".

Escopo desta rodada: só os formulários de Ativo (`ativo-form-imovel`, `ativo-form-carro`).
Lançamento, Contrato, Locatário, Login, Registro e Admin-usuários (que têm o mesmo problema de
mensagem genérica — ver `email`/`cpf`/`telefone` nesses formulários) ficam de fora, pra reaplicar
a convenção depois via issues separadas, mesmo padrão que a ADR-0007 usou.

**Regra de formato sobe pro backend primeiro, frontend espelha.** Mesmo padrão que
`anoFabricacao`/`anoModelo` já seguiam (`ativos-validadores.ts` espelhando
`Carro.AtualizarDadosDoCarro`): a fonte da verdade é o domínio, não o formulário. Como não existia
regra de formato nenhuma pra `Placa`/`Cep`/`Uf`, subimos a régua nos dois lados juntos, em vez de o
frontend inventar uma validação mais rígida que o backend não reforça (o que criaria uma regra
"de fachada", fácil de contornar chamando a API direto):

- `Carro.Placa`: regex pro formato antigo (`AAA-0000`) ou Mercosul (`AAA0A00`), aplicada sobre a
  placa já normalizada (trim + maiúsculas) — `Carro.cs`.
- `Endereco.Cep`: exatamente 8 dígitos (aceita com ou sem traço) — `Endereco.cs`.
- `Endereco.Uf`: lista real das 27 UFs brasileiras, não só `Length == 2` — `Endereco.cs`.

Aplicada igualmente em `Cadastrar` e `Atualizar` (mesmo guard clause nos dois caminhos, e não há
dado de produção real hoje que essa regra mais rígida arriscaria quebrar numa edição). `Matricula`
continua só obrigatória — o formato varia por cartório (já registrado na ADR-0007), não há regra
única pra espelhar. CPF/Telefone (Locatário) ficam de fora: são campos de um formulário fora do
escopo desta rodada.

**Frontend: uma mensagem por tipo de violação, resolvida por uma função pura.** Adicionamos
`placaValidator`, `cepValidator` e `ufValidator` em `ativos-validadores.ts`, espelhando as 3 regras
acima, e uma função `mensagemErro(control, mapaDeMensagens)` que olha `control.errors` e retorna a
mensagem da primeira chave presente no mapa (`{ required: '...', pattern: '...' }`) — aceita string
fixa ou uma função que recebe o payload do erro (ex: `{ minimo, maximo }` de
`anoFabricacaoValidator`), pra mensagem poder citar esse valor sem duplicá-lo. Os mapas de mensagem
por campo ficam centralizados perto de onde o campo é definido (`MENSAGENS_ATIVO_COMUM`/
`MENSAGENS_CARRO`/`MENSAGENS_IMOVEL` em `ativos-validadores.ts`, `MENSAGENS_FINANCIAMENTO` em
`financiamento-form.ts`, já que o fieldset de Financiamento é compartilhado pelos dois
formulários) — mesmo racional dos `ROTULOS_*`/`TEXTOS_AJUDA_*` já existentes em
`ativos-rotulos.ts`.

Optamos por uma função pura em vez de um componente `<app-campo>` que encapsulasse
label+input+aria: essa refatoração maior mexeria na estrutura que já existe nos dois formulários e
é arriscada demais pra caber junto da mudança de mensagem; fica como possível issue separada. Campo
com um único validador (`apelido`, `marca`, `matricula` etc.) continua com a mensagem fixa direto
no template, sem precisar de mapa — a função só entra onde há mais de um validador pra distinguir.

**Fora de escopo.** Erro de regra de negócio vindo do backend no submit (ex: duplicidade, se
existisse) continua só no banner genérico do topo (`erro()`/`mensagemErroHttp`) — não existe hoje
nenhuma regra de negócio conhecida (tipo placa duplicada) pra mapear pro campo certo; se aparecer
um caso real, vira issue própria. Os outros formulários do sistema (Lançamento, Contrato,
Locatário, Login, Registro, Admin-usuários) ficam de fora desta entrega.
