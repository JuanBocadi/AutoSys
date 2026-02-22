using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSys.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEditarVehiculos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Columna puede existir de una migración anterior – agregar solo si falta
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UserPermissions') AND name = 'EditarVehiculos')
                    ALTER TABLE [UserPermissions] ADD [EditarVehiculos] bit NOT NULL DEFAULT 0;
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('RolePermissions') AND name = 'EditarVehiculos')
                    ALTER TABLE [RolePermissions] ADD [EditarVehiculos] bit NOT NULL DEFAULT 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EditarVehiculos",
                table: "UserPermissions");

            migrationBuilder.DropColumn(
                name: "EditarVehiculos",
                table: "RolePermissions");
        }
    }
}
