using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DecalXeAPI.Migrations
{
    /// <inheritdoc />
    public partial class FullERDUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_DecalServices_DecalServiceServiceID",
                table: "OrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_Warranties_OrderID",
                table: "Warranties");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_DecalServiceServiceID",
                table: "OrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_OrderID",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Designs_OrderID",
                table: "Designs");

            migrationBuilder.DropColumn(
                name: "DecalServiceServiceID",
                table: "OrderDetails");

            migrationBuilder.AddColumn<string>(
                name: "CurrentStage",
                table: "Orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpectedArrivalTime",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCustomDecal",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "Orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleID",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualAreaUsed",
                table: "OrderDetails",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualLengthUsed",
                table: "OrderDetails",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualWidthUsed",
                table: "OrderDetails",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalCalculatedPrice",
                table: "OrderDetails",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "CarBrands",
                columns: table => new
                {
                    BrandID = table.Column<string>(type: "text", nullable: false),
                    BrandName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarBrands", x => x.BrandID);
                });

            migrationBuilder.CreateTable(
                name: "DesignComments",
                columns: table => new
                {
                    CommentID = table.Column<string>(type: "text", nullable: false),
                    CommentText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CommentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DesignID = table.Column<string>(type: "text", nullable: false),
                    SenderAccountID = table.Column<string>(type: "text", nullable: false),
                    ParentCommentID = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesignComments", x => x.CommentID);
                    table.ForeignKey(
                        name: "FK_DesignComments_Accounts_SenderAccountID",
                        column: x => x.SenderAccountID,
                        principalTable: "Accounts",
                        principalColumn: "AccountID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DesignComments_DesignComments_ParentCommentID",
                        column: x => x.ParentCommentID,
                        principalTable: "DesignComments",
                        principalColumn: "CommentID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DesignComments_Designs_DesignID",
                        column: x => x.DesignID,
                        principalTable: "Designs",
                        principalColumn: "DesignID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderCompletionImages",
                columns: table => new
                {
                    ImageID = table.Column<string>(type: "text", nullable: false),
                    ImageURL = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UploadDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrderID = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderCompletionImages", x => x.ImageID);
                    table.ForeignKey(
                        name: "FK_OrderCompletionImages_Orders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderStageHistories",
                columns: table => new
                {
                    OrderStageHistoryID = table.Column<string>(type: "text", nullable: false),
                    StageName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ChangeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrderID = table.Column<string>(type: "text", nullable: false),
                    ChangedByEmployeeID = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStageHistories", x => x.OrderStageHistoryID);
                    table.ForeignKey(
                        name: "FK_OrderStageHistories_Employees_ChangedByEmployeeID",
                        column: x => x.ChangedByEmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID");
                    table.ForeignKey(
                        name: "FK_OrderStageHistories_Orders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrintingPriceDetails",
                columns: table => new
                {
                    ServiceID = table.Column<string>(type: "text", nullable: false),
                    BasePricePerSqMeter = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MinLength = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MaxLength = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MinArea = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MaxArea = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ColorPricingFactor = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    PrintType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    FormulaDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrintingPriceDetails", x => x.ServiceID);
                    table.ForeignKey(
                        name: "FK_PrintingPriceDetails_DecalServices_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "DecalServices",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarModels",
                columns: table => new
                {
                    ModelID = table.Column<string>(type: "text", nullable: false),
                    ModelName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BrandID = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarModels", x => x.ModelID);
                    table.ForeignKey(
                        name: "FK_CarModels_CarBrands_BrandID",
                        column: x => x.BrandID,
                        principalTable: "CarBrands",
                        principalColumn: "BrandID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarModelDecalTemplates",
                columns: table => new
                {
                    CarModelDecalTemplateID = table.Column<string>(type: "text", nullable: false),
                    ModelID = table.Column<string>(type: "text", nullable: false),
                    TemplateID = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarModelDecalTemplates", x => x.CarModelDecalTemplateID);
                    table.ForeignKey(
                        name: "FK_CarModelDecalTemplates_CarModels_ModelID",
                        column: x => x.ModelID,
                        principalTable: "CarModels",
                        principalColumn: "ModelID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarModelDecalTemplates_DecalTemplates_TemplateID",
                        column: x => x.TemplateID,
                        principalTable: "DecalTemplates",
                        principalColumn: "TemplateID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerVehicles",
                columns: table => new
                {
                    VehicleID = table.Column<string>(type: "text", nullable: false),
                    LicensePlate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: true),
                    InitialKM = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CustomerID = table.Column<string>(type: "text", nullable: false),
                    ModelID = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerVehicles", x => x.VehicleID);
                    table.ForeignKey(
                        name: "FK_CustomerVehicles_CarModels_ModelID",
                        column: x => x.ModelID,
                        principalTable: "CarModels",
                        principalColumn: "ModelID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerVehicles_Customers_CustomerID",
                        column: x => x.CustomerID,
                        principalTable: "Customers",
                        principalColumn: "CustomerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_OrderID",
                table: "Warranties",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_VehicleID",
                table: "Orders",
                column: "VehicleID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_ServiceID",
                table: "OrderDetails",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_OrderID",
                table: "Feedbacks",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_Designs_OrderID",
                table: "Designs",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_CarModelDecalTemplates_ModelID",
                table: "CarModelDecalTemplates",
                column: "ModelID");

            migrationBuilder.CreateIndex(
                name: "IX_CarModelDecalTemplates_TemplateID",
                table: "CarModelDecalTemplates",
                column: "TemplateID");

            migrationBuilder.CreateIndex(
                name: "IX_CarModels_BrandID",
                table: "CarModels",
                column: "BrandID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerVehicles_CustomerID",
                table: "CustomerVehicles",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerVehicles_ModelID",
                table: "CustomerVehicles",
                column: "ModelID");

            migrationBuilder.CreateIndex(
                name: "IX_DesignComments_DesignID",
                table: "DesignComments",
                column: "DesignID");

            migrationBuilder.CreateIndex(
                name: "IX_DesignComments_ParentCommentID",
                table: "DesignComments",
                column: "ParentCommentID");

            migrationBuilder.CreateIndex(
                name: "IX_DesignComments_SenderAccountID",
                table: "DesignComments",
                column: "SenderAccountID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderCompletionImages_OrderID",
                table: "OrderCompletionImages",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStageHistories_ChangedByEmployeeID",
                table: "OrderStageHistories",
                column: "ChangedByEmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStageHistories_OrderID",
                table: "OrderStageHistories",
                column: "OrderID");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_DecalServices_ServiceID",
                table: "OrderDetails",
                column: "ServiceID",
                principalTable: "DecalServices",
                principalColumn: "ServiceID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_CustomerVehicles_VehicleID",
                table: "Orders",
                column: "VehicleID",
                principalTable: "CustomerVehicles",
                principalColumn: "VehicleID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_DecalServices_ServiceID",
                table: "OrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_CustomerVehicles_VehicleID",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "CarModelDecalTemplates");

            migrationBuilder.DropTable(
                name: "CustomerVehicles");

            migrationBuilder.DropTable(
                name: "DesignComments");

            migrationBuilder.DropTable(
                name: "OrderCompletionImages");

            migrationBuilder.DropTable(
                name: "OrderStageHistories");

            migrationBuilder.DropTable(
                name: "PrintingPriceDetails");

            migrationBuilder.DropTable(
                name: "CarModels");

            migrationBuilder.DropTable(
                name: "CarBrands");

            migrationBuilder.DropIndex(
                name: "IX_Warranties_OrderID",
                table: "Warranties");

            migrationBuilder.DropIndex(
                name: "IX_Orders_VehicleID",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_ServiceID",
                table: "OrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_OrderID",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Designs_OrderID",
                table: "Designs");

            migrationBuilder.DropColumn(
                name: "CurrentStage",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ExpectedArrivalTime",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsCustomDecal",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "VehicleID",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ActualAreaUsed",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "ActualLengthUsed",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "ActualWidthUsed",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "FinalCalculatedPrice",
                table: "OrderDetails");

            migrationBuilder.AddColumn<string>(
                name: "DecalServiceServiceID",
                table: "OrderDetails",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_OrderID",
                table: "Warranties",
                column: "OrderID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_DecalServiceServiceID",
                table: "OrderDetails",
                column: "DecalServiceServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_OrderID",
                table: "Feedbacks",
                column: "OrderID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Designs_OrderID",
                table: "Designs",
                column: "OrderID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_DecalServices_DecalServiceServiceID",
                table: "OrderDetails",
                column: "DecalServiceServiceID",
                principalTable: "DecalServices",
                principalColumn: "ServiceID");
        }
    }
}
