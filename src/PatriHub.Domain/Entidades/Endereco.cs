namespace PatriHub.Domain.Entidades;

/// <summary>Endereço de um Imóvel. Ver 01-SPEC-FUNCIONAL.md §4.3.</summary>
public sealed class Endereco
{
    /// <summary>As 27 UFs brasileiras — ver docs/adr/0008. Antes só se checava `Length == 2`.</summary>
    private static readonly HashSet<string> UfsValidas =
    [
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG", "PA", "PB",
        "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO",
    ];

    public string Rua { get; private set; } = string.Empty;
    public string Numero { get; private set; } = string.Empty;
    public string? Complemento { get; private set; }
    public string Bairro { get; private set; } = string.Empty;
    public string Cidade { get; private set; } = string.Empty;
    public string Uf { get; private set; } = string.Empty;
    public string Cep { get; private set; } = string.Empty;

    private Endereco()
    {
        // EF Core
    }

    private Endereco(string rua, string numero, string? complemento, string bairro, string cidade, string uf, string cep)
    {
        Rua = rua;
        Numero = numero;
        Complemento = complemento;
        Bairro = bairro;
        Cidade = cidade;
        Uf = uf;
        Cep = cep;
    }

    public static Endereco Criar(string rua, string numero, string? complemento, string bairro, string cidade, string uf, string cep)
    {
        ExigirPreenchido(rua, nameof(rua));
        ExigirPreenchido(numero, nameof(numero));
        ExigirPreenchido(bairro, nameof(bairro));
        ExigirPreenchido(cidade, nameof(cidade));
        ExigirPreenchido(uf, nameof(uf));
        ExigirPreenchido(cep, nameof(cep));

        if (uf.Trim().Length != 2)
        {
            throw new ArgumentException("UF deve ter 2 letras.", nameof(uf));
        }

        if (!UfsValidas.Contains(uf.Trim().ToUpperInvariant()))
        {
            throw new ArgumentException("UF inválida.", nameof(uf));
        }

        var cepDigitos = new string(cep.Where(char.IsDigit).ToArray());
        if (cepDigitos.Length != 8)
        {
            throw new ArgumentException("CEP deve conter 8 dígitos.", nameof(cep));
        }

        return new Endereco(
            rua.Trim(),
            numero.Trim(),
            string.IsNullOrWhiteSpace(complemento) ? null : complemento.Trim(),
            bairro.Trim(),
            cidade.Trim(),
            uf.Trim().ToUpperInvariant(),
            cep.Trim());
    }

    private static void ExigirPreenchido(string valor, string nomeCampo)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException($"{nomeCampo} não pode ser vazio.", nomeCampo);
        }
    }
}
