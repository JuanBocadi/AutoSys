using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSys.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAuditLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EntidadId = table.Column<int>(type: "int", nullable: true),
                    EntidadNombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DireccionIP = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Categoria",
                table: "AuditLogs",
                column: "Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Fecha",
                table: "AuditLogs",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Usuario",
                table: "AuditLogs",
                column: "Usuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");
        }
    }
}
