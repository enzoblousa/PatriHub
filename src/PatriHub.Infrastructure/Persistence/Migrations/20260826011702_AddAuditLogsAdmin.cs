using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatriHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogsAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogsAdmin",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminUsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioAlvoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Recurso = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RecursoId = table.Column<Guid>(type: "uuid", nullable: true),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogsAdmin", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogsAdmin_UsuarioAlvoId_CriadoEm",
                table: "AuditLogsAdmin",
                columns: new[] { "UsuarioAlvoId", "CriadoEm" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogsAdmin");
        }
    }
}
