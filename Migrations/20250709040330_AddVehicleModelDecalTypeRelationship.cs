using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DecalXeAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleModelDecalTypeRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VehicleModelDecalTypes",
                columns: table => new
                {
                    VehicleModelDecalTypeID = table.Column<string>(type: "text", nullable: false),
                    ModelID = table.Column<string>(type: "text", nullable: false),
                    DecalTypeID = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleModelDecalTypes", x => x.VehicleModelDecalTypeID);
                    table.ForeignKey(
                        name: "FK_VehicleModelDecalTypes_DecalTypes_DecalTypeID",
                        column: x => x.DecalTypeID,
                        principalTable: "DecalTypes",
                        principalColumn: "DecalTypeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VehicleModelDecalTypes_VehicleModels_ModelID",
                        column: x => x.ModelID,
                        principalTable: "VehicleModels",
                        principalColumn: "ModelID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleModelDecalTypes_DecalTypeID",
                table: "VehicleModelDecalTypes",
                column: "DecalTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleModelDecalTypes_ModelID",
                table: "VehicleModelDecalTypes",
                column: "ModelID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleModelDecalTypes");
        }
    }
}
