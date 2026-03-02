using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSys.Migrations
{
    /// <inheritdoc />
    public partial class AddStockIdToDetalleFactura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StockId",
                table: "DetallesFactura",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetallesFactura_StockId",
                table: "DetallesFactura",
                column: "StockId");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesFactura_Stock_StockId",
                table: "DetallesFactura",
                column: "StockId",
                principalTable: "Stock",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetallesFactura_Stock_StockId",
                table: "DetallesFactura");

            migrationBuilder.DropIndex(
                name: "IX_DetallesFactura_StockId",
                table: "DetallesFactura");

            migrationBuilder.DropColumn(
                name: "StockId",
                table: "DetallesFactura");
        }
    }
}
