using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSys.Migrations
{
    /// <inheritdoc />
    public partial class MultipleServiciosPorIngreso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ingresos_ServiciosFijos_ServicioFijoId",
                table: "Ingresos");

            migrationBuilder.DropIndex(
                name: "IX_Ingresos_ServicioFijoId",
                table: "Ingresos");

            migrationBuilder.DropColumn(
                name: "ServicioFijoId",
                table: "Ingresos");

            migrationBuilder.AlterColumn<string>(
                name: "DiagnosticoInicial",
                table: "Ingresos",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.CreateTable(
                name: "IngresoServicio",
                columns: table => new
                {
                    IngresoId = table.Column<int>(type: "int", nullable: false),
                    ServicioFijoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngresoServicio", x => new { x.IngresoId, x.ServicioFijoId });
                    table.ForeignKey(
                        name: "FK_IngresoServicio_Ingresos_IngresoId",
                        column: x => x.IngresoId,
                        principalTable: "Ingresos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IngresoServicio_ServiciosFijos_ServicioFijoId",
                        column: x => x.ServicioFijoId,
                        principalTable: "ServiciosFijos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IngresoServicio_ServicioFijoId",
                table: "IngresoServicio",
                column: "ServicioFijoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IngresoServicio");

            migrationBuilder.AlterColumn<string>(
                name: "DiagnosticoInicial",
                table: "Ingresos",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServicioFijoId",
                table: "Ingresos",
                type: "int",
                nullable: true);

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
    }
}
