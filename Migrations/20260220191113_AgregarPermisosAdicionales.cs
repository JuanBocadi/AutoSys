using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSys.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPermisosAdicionales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AnularFacturas",
                table: "UserPermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EditarVehiculos",
                table: "UserPermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EliminarClientes",
                table: "UserPermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AnularFacturas",
                table: "RolePermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EditarVehiculos",
                table: "RolePermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EliminarClientes",
                table: "RolePermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnularFacturas",
                table: "UserPermissions");

            migrationBuilder.DropColumn(
                name: "EditarVehiculos",
                table: "UserPermissions");

            migrationBuilder.DropColumn(
                name: "EliminarClientes",
                table: "UserPermissions");

            migrationBuilder.DropColumn(
                name: "AnularFacturas",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "EditarVehiculos",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "EliminarClientes",
                table: "RolePermissions");
        }
    }
}
