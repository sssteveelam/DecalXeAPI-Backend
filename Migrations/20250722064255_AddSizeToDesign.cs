using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DecalXeAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddSizeToDesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "Designs",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Size",
                table: "Designs");
        }
    }
}
