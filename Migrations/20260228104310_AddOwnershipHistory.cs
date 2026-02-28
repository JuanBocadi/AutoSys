using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSys.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnershipHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TransferirVehiculo",
                table: "UserPermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TransferirVehiculo",
                table: "RolePermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ClienteId",
                table: "Ingresos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HistorialesPropietarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehiculoId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    FechaDesde = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaHasta = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EsPropietarioActual = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialesPropietarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialesPropietarios_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistorialesPropietarios_Vehiculos_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "Vehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ingresos_ClienteId",
                table: "Ingresos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialesPropietarios_ClienteId",
                table: "HistorialesPropietarios",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialesPropietarios_VehiculoId",
                table: "HistorialesPropietarios",
                column: "VehiculoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ingresos_Clientes_ClienteId",
                table: "Ingresos",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ingresos_Clientes_ClienteId",
                table: "Ingresos");

            migrationBuilder.DropTable(
                name: "HistorialesPropietarios");

            migrationBuilder.DropIndex(
                name: "IX_Ingresos_ClienteId",
                table: "Ingresos");

            migrationBuilder.DropColumn(
                name: "TransferirVehiculo",
                table: "UserPermissions");

            migrationBuilder.DropColumn(
                name: "TransferirVehiculo",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "Ingresos");
        }
    }
}
