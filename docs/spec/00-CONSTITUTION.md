# PatriHub — Constituição do Projeto

## Missão
Dar a proprietários pessoa física de imóveis e/ou carros alugados uma visão financeira clara
do próprio patrimônio: quanto cada ativo dá de lucro, qual o retorno (ROI/yield) e se vale a
pena continuar com ele, tudo em um único lugar.

## Princípios do MVP
1. **Manual antes de automático.** Lançamentos financeiros são digitados pelo usuário no MVP;
   importação (CSV/OFX) e integrações bancárias (Open Finance) ficam para depois.
2. **Simples antes de completo.** Preferir cobrir bem o fluxo essencial (cadastrar ativo →
   lançar receita/despesa → ver lucro) a cobrir muitos casos de borda.
3. **Privacidade por padrão (LGPD).** Dados financeiros e pessoais (CPF de locatário, valores)
   são sensíveis; acesso é restrito ao dono do dado. Admin é a única exceção: tem leitura
   (nunca edição/exclusão) de ativos e lançamentos de qualquer usuário para fins de suporte,
   com todo acesso registrado em log de auditoria — ver [ADR-0002](../adr/0002-admin-acesso-leitura-com-auditoria.md).
4. **Sem custo de terceiros desnecessário no MVP.** Autenticação self-hosted, sem cobrança
   ainda, sem integrações pagas — o objetivo é validar o produto com early adopters.
5. **Um monólito modular, não microsserviços.** Complexidade de infraestrutura não se
   justifica no estágio de validação de produto.

## Fora de escopo do MVP (explícito)
- Cobrança/assinatura (planos pagos, Stripe ou similar)
- Multiusuário por conta (convidar contador, co-proprietário)
- Notificações (email/WhatsApp/push)
- Relatórios exportáveis (PDF/Excel)
- Anexos de documentos (contrato PDF, fotos, comprovantes)
- Importação de extrato (CSV/OFX) e Open Finance
- App mobile nativo
- Integrações externas (FIPE API, gateways de pagamento, etc.)

## Stack fixada
- **Backend:** .NET (ASP.NET Core Web API)
- **Frontend:** Angular
- **Banco de dados:** PostgreSQL
- **Autenticação:** ASP.NET Core Identity + JWT, self-hosted
- **Containers:** Docker desde o início
- **Hospedagem:** Azure (App Service / Container Apps)
- **Mercado/idioma/moeda:** Brasil, pt-BR, BRL — únicos suportados no MVP
