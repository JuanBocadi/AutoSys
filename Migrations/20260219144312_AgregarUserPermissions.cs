using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSys.Migrations
{
    /// <inheritdoc />
    public partial class AgregarUserPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    VerClientes = table.Column<bool>(type: "bit", nullable: false),
                    CrearClientes = table.Column<bool>(type: "bit", nullable: false),
                    EditarClientes = table.Column<bool>(type: "bit", nullable: false),
                    VerVehiculos = table.Column<bool>(type: "bit", nullable: false),
                    CrearVehiculos = table.Column<bool>(type: "bit", nullable: false),
                    VerIngresos = table.Column<bool>(type: "bit", nullable: false),
                    CrearIngresos = table.Column<bool>(type: "bit", nullable: false),
                    ActualizarEstadoIngresos = table.Column<bool>(type: "bit", nullable: false),
                    VerReparaciones = table.Column<bool>(type: "bit", nullable: false),
                    VerStock = table.Column<bool>(type: "bit", nullable: false),
                    CrearStock = table.Column<bool>(type: "bit", nullable: false),
                    EditarStock = table.Column<bool>(type: "bit", nullable: false),
                    AjustarStock = table.Column<bool>(type: "bit", nullable: false),
                    VerFacturacion = table.Column<bool>(type: "bit", nullable: false),
                    CrearFacturas = table.Column<bool>(type: "bit", nullable: false),
                    VerReportes = table.Column<bool>(type: "bit", nullable: false),
                    UltimaModificacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificadoPor = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPermissions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPermissions_UserId",
                table: "UserPermissions",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPermissions");
        }
    }
}
