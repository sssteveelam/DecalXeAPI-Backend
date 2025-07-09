using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DecalXeAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceToVehicleModelDecalType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "VehicleModelDecalTypes",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "VehicleModelDecalTypes");
        }
    }
}
