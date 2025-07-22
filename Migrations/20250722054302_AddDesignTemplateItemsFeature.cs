using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DecalXeAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddDesignTemplateItemsFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DesignTemplateItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ItemName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PlacementPosition = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Width = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    Height = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    DesignId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesignTemplateItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DesignTemplateItems_Designs_DesignId",
                        column: x => x.DesignId,
                        principalTable: "Designs",
                        principalColumn: "DesignID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DesignTemplateItems_DesignId",
                table: "DesignTemplateItems",
                column: "DesignId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DesignTemplateItems");
        }
    }
}
