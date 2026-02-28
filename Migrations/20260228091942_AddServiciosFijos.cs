using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSys.Migrations
{
    /// <inheritdoc />
    public partial class AddServiciosFijos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "GestionarServiciosFijos",
                table: "UserPermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "GestionarServiciosFijos",
                table: "RolePermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ServicioFijoId",
                table: "Ingresos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ServiciosFijos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DescripcionPredeterminada = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrecioSugerido = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiciosFijos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ingresos_ServicioFijoId",
                table: "Ingresos",
                column: "ServicioFijoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ingresos_ServiciosFijos_ServicioFijoId",
                table: "Ingresos",
                column: "ServicioFijoId",
                principalTable: "ServiciosFijos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ingresos_ServiciosFijos_ServicioFijoId",
                table: "Ingresos");

            migrationBuilder.DropTable(
                name: "ServiciosFijos");

            migrationBuilder.DropIndex(
                name: "IX_Ingresos_ServicioFijoId",
                table: "Ingresos");

            migrationBuilder.DropColumn(
                name: "GestionarServiciosFijos",
                table: "UserPermissions");

            migrationBuilder.DropColumn(
                name: "GestionarServiciosFijos",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "ServicioFijoId",
                table: "Ingresos");
        }
    }
}
