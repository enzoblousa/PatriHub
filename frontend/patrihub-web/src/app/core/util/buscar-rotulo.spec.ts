import { buscarRotulo } from './buscar-rotulo';

describe('buscarRotulo', () => {
  const lista = [
    { id: 'ativo-1', apelido: 'Apê Centro' },
    { id: 'ativo-2', apelido: 'Corolla' },
  ];

  it('retorna o rótulo extraído do item encontrado', () => {
    expect(buscarRotulo(lista, 'ativo-2', (a) => a.apelido)).toBe('Corolla');
  });

  it('cai de volta pro próprio id quando o item não existe na lista', () => {
    expect(buscarRotulo(lista, 'ativo-3', (a) => a.apelido)).toBe('ativo-3');
  });

  it('cai de volta pro próprio id quando a lista está vazia (ainda carregando)', () => {
    expect(buscarRotulo([], 'ativo-1', (a: { id: string; apelido: string }) => a.apelido)).toBe(
      'ativo-1',
    );
  });
});
