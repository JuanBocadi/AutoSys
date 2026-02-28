using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSys.Migrations
{
    /// <inheritdoc />
    public partial class AddBackupsAndAuditoriaPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "GestionarBackups",
                table: "UserPermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "VerAuditoria",
                table: "UserPermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "VerBackups",
                table: "UserPermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "GestionarBackups",
                table: "RolePermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "VerAuditoria",
                table: "RolePermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "VerBackups",
                table: "RolePermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GestionarBackups",
                table: "UserPermissions");

            migrationBuilder.DropColumn(
                name: "VerAuditoria",
                table: "UserPermissions");

            migrationBuilder.DropColumn(
                name: "VerBackups",
                table: "UserPermissions");

            migrationBuilder.DropColumn(
                name: "GestionarBackups",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "VerAuditoria",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "VerBackups",
                table: "RolePermissions");
        }
    }
}
