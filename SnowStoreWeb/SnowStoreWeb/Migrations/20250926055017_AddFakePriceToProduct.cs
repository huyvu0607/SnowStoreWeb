using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SnowStoreWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddFakePriceToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FakePrice",
                table: "Products",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FakePrice",
                table: "Products");
        }
    }
}
