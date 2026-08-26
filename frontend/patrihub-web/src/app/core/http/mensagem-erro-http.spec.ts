import { HttpErrorResponse } from '@angular/common/http';

import { mensagemErroHttp } from './mensagem-erro-http';

describe('mensagemErroHttp', () => {
  it('extrai a mensagem de erro estruturada do backend', () => {
    const erro = new HttpErrorResponse({ error: { erro: 'Apelido não pode ser vazio.' } });

    expect(mensagemErroHttp(erro, 'padrão')).toBe('Apelido não pode ser vazio.');
  });

  it('usa a mensagem padrão quando o corpo do erro não é estruturado', () => {
    const erro = new HttpErrorResponse({ error: 'texto qualquer' });

    expect(mensagemErroHttp(erro, 'padrão')).toBe('padrão');
  });

  it('usa a mensagem padrão pra erro que não é HttpErrorResponse', () => {
    expect(mensagemErroHttp(new Error('falha de rede'), 'padrão')).toBe('padrão');
  });
});
