using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatriHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocatariosEContratos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Locatarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    Telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locatarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contratos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    AtivoId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocatarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValorAluguelMensal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiaVencimento = table.Column<int>(type: "integer", nullable: false),
                    DataInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    DataFim = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contratos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contratos_Ativos_AtivoId",
                        column: x => x.AtivoId,
                        principalTable: "Ativos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contratos_Locatarios_LocatarioId",
                        column: x => x.LocatarioId,
                        principalTable: "Locatarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lancamentos_ContratoId",
                table: "Lancamentos",
                column: "ContratoId");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_AtivoId_Status",
                table: "Contratos",
                columns: new[] { "AtivoId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_LocatarioId",
                table: "Contratos",
                column: "LocatarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_UsuarioId",
                table: "Contratos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Locatarios_UsuarioId",
                table: "Locatarios",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lancamentos_Contratos_ContratoId",
                table: "Lancamentos",
                column: "ContratoId",
                principalTable: "Contratos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lancamentos_Contratos_ContratoId",
                table: "Lancamentos");

            migrationBuilder.DropTable(
                name: "Contratos");

            migrationBuilder.DropTable(
                name: "Locatarios");

            migrationBuilder.DropIndex(
                name: "IX_Lancamentos_ContratoId",
                table: "Lancamentos");
        }
    }
}
