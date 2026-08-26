/**
 * Busca um item por `id` numa lista e extrai um rótulo dele — usado pra exibir, por exemplo,
 * o apelido do Ativo de um Lançamento ou o nome do Locatário de um Contrato, em vez do id cru.
 * Cai de volta pro próprio `id` quando a lista ainda não carregou ou o item não existe mais.
 */
export function buscarRotulo<T extends { id: string }>(
  lista: readonly T[],
  id: string,
  extrairRotulo: (item: T) => string,
): string {
  const item = lista.find((candidato) => candidato.id === id);
  return item ? extrairRotulo(item) : id;
}
