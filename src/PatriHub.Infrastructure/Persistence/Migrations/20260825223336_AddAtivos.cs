using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatriHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAtivos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ativos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Apelido = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DataAquisicao = table.Column<DateOnly>(type: "date", nullable: false),
                    ValorAquisicao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorMercadoAtual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Financiamento_ValorParcela = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Financiamento_SaldoDevedor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Financiamento_TaxaJurosAnual = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: true),
                    Financiamento_ParcelasRestantes = table.Column<int>(type: "integer", nullable: true),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExcluidoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ativos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Carros",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Placa = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Marca = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Modelo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AnoFabricacao = table.Column<int>(type: "integer", nullable: false),
                    AnoModelo = table.Column<int>(type: "integer", nullable: false),
                    ValorFipeAtual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Km = table.Column<decimal>(type: "numeric(10,1)", precision: 10, scale: 1, nullable: false),
                    ConsumoMedio = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Carros_Ativos_Id",
                        column: x => x.Id,
                        principalTable: "Ativos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Imoveis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Endereco_Rua = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Endereco_Numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Endereco_Complemento = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Endereco_Bairro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Endereco_Cidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Endereco_Uf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Endereco_Cep = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    TipoImovel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AreaM2 = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Matricula = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ValorIptuMensal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorCondominioMensal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Imoveis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Imoveis_Ativos_Id",
                        column: x => x.Id,
                        principalTable: "Ativos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ativos_UsuarioId_ExcluidoEm",
                table: "Ativos",
                columns: new[] { "UsuarioId", "ExcluidoEm" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Carros");

            migrationBuilder.DropTable(
                name: "Imoveis");

            migrationBuilder.DropTable(
                name: "Ativos");
        }
    }
}
