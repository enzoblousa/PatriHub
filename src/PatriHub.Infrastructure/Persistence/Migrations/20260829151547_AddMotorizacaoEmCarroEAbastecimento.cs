using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatriHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMotorizacaoEmCarroEAbastecimento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Motorizacao",
                table: "Carros",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Combustao");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Motorizacao",
                table: "Carros");
        }
    }
}
