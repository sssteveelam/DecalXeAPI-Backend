using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DecalXeAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddStageToOrderStageHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Stage",
                table: "OrderStageHistories",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Stage",
                table: "OrderStageHistories");
        }
    }
}
