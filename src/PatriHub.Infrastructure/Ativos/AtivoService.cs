using Microsoft.EntityFrameworkCore;
using PatriHub.Application.Ativos;
using PatriHub.Application.Common;
using PatriHub.Domain.Entidades;
using PatriHub.Infrastructure.Persistence;

namespace PatriHub.Infrastructure.Ativos;

/// <summary>
/// Toda consulta filtra por `UsuarioId` diretamente na query (nunca checa dono depois de
/// carregar) — um Ativo de outro usuário simplesmente não aparece, o que satisfaz o
/// 404 de isolamento por conta (ver 01-SPEC-FUNCIONAL.md §7).
/// </summary>
public sealed class AtivoService(PatriHubDbContext db) : IAtivoService
{
    public async Task<ResultadoOperacao<AtivoDetalheDto>> CriarImovelAsync(Guid usuarioId, CriarImovelRequest request)
    {
        try
        {
            var imovel = Imovel.Cadastrar(
                usuarioId,
                request.Apelido,
                request.DataAquisicao,
                request.ValorAquisicao,
                request.ValorMercadoAtual,
                MapearEndereco(request.Endereco),
                request.TipoImovel,
                request.AreaM2,
                request.Matricula,
                request.ValorIptuMensal,
                request.ValorCondominioMensal,
                MapearFinanciamento(request.Financiamento));

            db.Ativos.Add(imovel);
            await db.SaveChangesAsync();

            return ResultadoOperacao<AtivoDetalheDto>.ComSucesso(MapearDetalhe(imovel, lucroDoMes: 0m));
        }
        catch (ArgumentException ex)
        {
            return ResultadoOperacao<AtivoDetalheDto>.ComErro(ex.Message, TipoErroOperacao.Validacao);
        }
    }

    public async Task<ResultadoOperacao<AtivoDetalheDto>> CriarCarroAsync(Guid usuarioId, CriarCarroRequest request)
    {
        try
        {
            var carro = Carro.Cadastrar(
                usuarioId,
                request.Apelido,
                request.DataAquisicao,
                request.ValorAquisicao,
                request.ValorMercadoAtual,
                request.Placa,
                request.Marca,
                request.Modelo,
                request.AnoFabricacao,
                request.AnoModelo,
                request.ValorFipeAtual,
                request.Km,
                request.ConsumoMedio,
                MapearFinanciamento(request.Financiamento));

            db.Ativos.Add(carro);
            await db.SaveChangesAsync();

            return ResultadoOperacao<AtivoDetalheDto>.ComSucesso(MapearDetalhe(carro, lucroDoMes: 0m));
        }
        catch (ArgumentException ex)
        {
            return ResultadoOperacao<AtivoDetalheDto>.ComErro(ex.Message, TipoErroOperacao.Validacao);
        }
    }

    public async Task<ResultadoOperacao<AtivoDetalheDto>> AtualizarImovelAsync(Guid usuarioId, Guid ativoId, AtualizarImovelRequest request)
    {
        var ativo = await BuscarAtivoDoUsuarioAsync(usuarioId, ativoId);
        if (ativo is not Imovel imovel)
        {
            return ResultadoOperacao<AtivoDetalheDto>.ComErro("Imóvel não encontrado.", TipoErroOperacao.NaoEncontrado);
        }

        try
        {
            imovel.Atualizar(
                request.Apelido,
                request.DataAquisicao,
                request.ValorAquisicao,
                request.ValorMercadoAtual,
                MapearEndereco(request.Endereco),
                request.TipoImovel,
                request.AreaM2,
                request.Matricula,
                request.ValorIptuMensal,
                request.ValorCondominioMensal,
                MapearFinanciamento(request.Financiamento));
        }
        catch (ArgumentException ex)
        {
            return ResultadoOperacao<AtivoDetalheDto>.ComErro(ex.Message, TipoErroOperacao.Validacao);
        }

        await db.SaveChangesAsync();
        return ResultadoOperacao<AtivoDetalheDto>.ComSucesso(MapearDetalhe(imovel, lucroDoMes: 0m));
    }

    public async Task<ResultadoOperacao<AtivoDetalheDto>> AtualizarCarroAsync(Guid usuarioId, Guid ativoId, AtualizarCarroRequest request)
    {
        var ativo = await BuscarAtivoDoUsuarioAsync(usuarioId, ativoId);
        if (ativo is not Carro carro)
        {
            return ResultadoOperacao<AtivoDetalheDto>.ComErro("Carro não encontrado.", TipoErroOperacao.NaoEncontrado);
        }

        try
        {
            carro.Atualizar(
                request.Apelido,
                request.DataAquisicao,
                request.ValorAquisicao,
                request.ValorMercadoAtual,
                request.Placa,
                request.Marca,
                request.Modelo,
                request.AnoFabricacao,
                request.AnoModelo,
                request.ValorFipeAtual,
                request.Km,
                request.ConsumoMedio,
                MapearFinanciamento(request.Financiamento));
        }
        catch (ArgumentException ex)
        {
            return ResultadoOperacao<AtivoDetalheDto>.ComErro(ex.Message, TipoErroOperacao.Validacao);
        }

        await db.SaveChangesAsync();
        return ResultadoOperacao<AtivoDetalheDto>.ComSucesso(MapearDetalhe(carro, lucroDoMes: 0m));
    }

    public async Task<ResultadoOperacao<AtivoDetalheDto>> MarcarStatusAsync(Guid usuarioId, Guid ativoId, MarcarStatusAtivoRequest request)
    {
        var ativo = await BuscarAtivoDoUsuarioAsync(usuarioId, ativoId);
        if (ativo is null)
        {
            return ResultadoOperacao<AtivoDetalheDto>.ComErro("Ativo não encontrado.", TipoErroOperacao.NaoEncontrado);
        }

        try
        {
            ativo.MarcarStatusManual(request.Status);
        }
        catch (ArgumentException ex)
        {
            return ResultadoOperacao<AtivoDetalheDto>.ComErro(ex.Message, TipoErroOperacao.Validacao);
        }

        await db.SaveChangesAsync();
        return ResultadoOperacao<AtivoDetalheDto>.ComSucesso(MapearDetalhe(ativo, lucroDoMes: 0m));
    }

    public async Task<ResultadoOperacao> ExcluirAsync(Guid usuarioId, Guid ativoId)
    {
        var ativo = await BuscarAtivoDoUsuarioAsync(usuarioId, ativoId);
        if (ativo is null)
        {
            return ResultadoOperacao.ComErro("Ativo não encontrado.", TipoErroOperacao.NaoEncontrado);
        }

        ativo.Excluir();
        await db.SaveChangesAsync();
        return ResultadoOperacao.ComSucesso();
    }

    public async Task<IReadOnlyList<AtivoResumoDto>> ListarAsync(Guid usuarioId)
    {
        var ativos = await db.Ativos
            .Where(a => a.UsuarioId == usuarioId && a.ExcluidoEm == null)
            .OrderByDescending(a => a.CriadoEm)
            .ToListAsync();

        // Lucro do mês fica 0 até o ticket de Lançamentos (#4) existir — ver AC do ticket #3.
        return ativos
            .Select(a => new AtivoResumoDto(a.Id, a.Apelido, a.Tipo, a.Status, a.ValorMercadoAtual, LucroDoMes: 0m))
            .ToList();
    }

    public async Task<ResultadoOperacao<AtivoDetalheDto>> ObterDetalheAsync(Guid usuarioId, Guid ativoId)
    {
        var ativo = await BuscarAtivoDoUsuarioAsync(usuarioId, ativoId);
        if (ativo is null)
        {
            return ResultadoOperacao<AtivoDetalheDto>.ComErro("Ativo não encontrado.", TipoErroOperacao.NaoEncontrado);
        }

        return ResultadoOperacao<AtivoDetalheDto>.ComSucesso(MapearDetalhe(ativo, lucroDoMes: 0m));
    }

    private Task<Ativo?> BuscarAtivoDoUsuarioAsync(Guid usuarioId, Guid ativoId) =>
        db.Ativos.FirstOrDefaultAsync(a => a.Id == ativoId && a.UsuarioId == usuarioId && a.ExcluidoEm == null);

    private static Endereco MapearEndereco(EnderecoDto dto) =>
        Endereco.Criar(dto.Rua, dto.Numero, dto.Complemento, dto.Bairro, dto.Cidade, dto.Uf, dto.Cep);

    private static DadosFinanciamento? MapearFinanciamento(DadosFinanciamentoDto? dto) =>
        dto is null
            ? null
            : DadosFinanciamento.Criar(dto.ValorParcela, dto.SaldoDevedor, dto.TaxaJurosAnual, dto.ParcelasRestantes);

    private static DadosFinanciamentoDto? MapearFinanciamentoDto(DadosFinanciamento? financiamento) =>
        financiamento is null
            ? null
            : new DadosFinanciamentoDto(financiamento.ValorParcela, financiamento.SaldoDevedor, financiamento.TaxaJurosAnual, financiamento.ParcelasRestantes);

    private static EnderecoDto MapearEnderecoDto(Endereco endereco) =>
        new(endereco.Rua, endereco.Numero, endereco.Complemento, endereco.Bairro, endereco.Cidade, endereco.Uf, endereco.Cep);

    private static AtivoDetalheDto MapearDetalhe(Ativo ativo, decimal lucroDoMes) => new(
        ativo.Id,
        ativo.Apelido,
        ativo.Tipo,
        ativo.Status,
        ativo.DataAquisicao,
        ativo.ValorAquisicao,
        ativo.ValorMercadoAtual,
        ativo.Financiado,
        MapearFinanciamentoDto(ativo.Financiamento),
        ativo.CriadoEm,
        ativo.AtualizadoEm,
        ativo is Imovel imovel
            ? new ImovelDetalheDto(MapearEnderecoDto(imovel.Endereco), imovel.TipoImovel, imovel.AreaM2, imovel.Matricula, imovel.ValorIptuMensal, imovel.ValorCondominioMensal)
            : null,
        ativo is Carro carro
            ? new CarroDetalheDto(carro.Placa, carro.Marca, carro.Modelo, carro.AnoFabricacao, carro.AnoModelo, carro.ValorFipeAtual, carro.Km, carro.ConsumoMedio)
            : null);
}
