using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DecalXeAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductAndPricingAndCsr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomServiceRequests");

            migrationBuilder.DropTable(
                name: "PrintingPriceDetails");

            migrationBuilder.DropTable(
                name: "ServiceVehicleModelProducts");

            migrationBuilder.DropTable(
                name: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomServiceRequests",
                columns: table => new
                {
                    CustomRequestID = table.Column<string>(type: "text", nullable: false),
                    CustomerID = table.Column<string>(type: "text", nullable: false),
                    OrderID = table.Column<string>(type: "text", nullable: true),
                    SalesEmployeeID = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DesiredCompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EstimatedCost = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    EstimatedWorkUnits = table.Column<int>(type: "integer", nullable: true),
                    ReferenceImageURL = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequestStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomServiceRequests", x => x.CustomRequestID);
                    table.ForeignKey(
                        name: "FK_CustomServiceRequests_Customers_CustomerID",
                        column: x => x.CustomerID,
                        principalTable: "Customers",
                        principalColumn: "CustomerID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomServiceRequests_Employees_SalesEmployeeID",
                        column: x => x.SalesEmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID");
                    table.ForeignKey(
                        name: "FK_CustomServiceRequests_Orders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID");
                });

            migrationBuilder.CreateTable(
                name: "PrintingPriceDetails",
                columns: table => new
                {
                    ServiceID = table.Column<string>(type: "text", nullable: false),
                    BasePricePerSqMeter = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ColorPricingFactor = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    FormulaDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    MaxArea = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MaxLength = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MinArea = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MinLength = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PrintType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
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
                name: "Products",
                columns: table => new
                {
                    ProductID = table.Column<string>(type: "text", nullable: false),
                    CategoryID = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ProductName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductID);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "Categories",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceVehicleModelProducts",
                columns: table => new
                {
                    ServiceID = table.Column<string>(type: "text", nullable: false),
                    VehicleModelID = table.Column<string>(type: "text", nullable: false),
                    ProductID = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceVehicleModelProducts", x => new { x.ServiceID, x.VehicleModelID, x.ProductID });
                    table.ForeignKey(
                        name: "FK_ServiceVehicleModelProducts_DecalServices_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "DecalServices",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceVehicleModelProducts_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceVehicleModelProducts_VehicleModels_VehicleModelID",
                        column: x => x.VehicleModelID,
                        principalTable: "VehicleModels",
                        principalColumn: "ModelID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomServiceRequests_CustomerID",
                table: "CustomServiceRequests",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomServiceRequests_OrderID",
                table: "CustomServiceRequests",
                column: "OrderID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomServiceRequests_SalesEmployeeID",
                table: "CustomServiceRequests",
                column: "SalesEmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryID",
                table: "Products",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceVehicleModelProducts_ProductID",
                table: "ServiceVehicleModelProducts",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceVehicleModelProducts_VehicleModelID",
                table: "ServiceVehicleModelProducts",
                column: "VehicleModelID");
        }
    }
}
