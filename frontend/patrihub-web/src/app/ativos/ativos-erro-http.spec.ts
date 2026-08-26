import { HttpErrorResponse } from '@angular/common/http';

import { mensagemErroAtivo } from './ativos-erro-http';

describe('mensagemErroAtivo', () => {
  it('extrai a mensagem de erro estruturada do backend', () => {
    const erro = new HttpErrorResponse({ error: { erro: 'Apelido não pode ser vazio.' } });

    expect(mensagemErroAtivo(erro, 'padrão')).toBe('Apelido não pode ser vazio.');
  });

  it('usa a mensagem padrão quando o corpo do erro não é estruturado', () => {
    const erro = new HttpErrorResponse({ error: 'texto qualquer' });

    expect(mensagemErroAtivo(erro, 'padrão')).toBe('padrão');
  });

  it('usa a mensagem padrão pra erro que não é HttpErrorResponse', () => {
    expect(mensagemErroAtivo(new Error('falha de rede'), 'padrão')).toBe('padrão');
  });
});
