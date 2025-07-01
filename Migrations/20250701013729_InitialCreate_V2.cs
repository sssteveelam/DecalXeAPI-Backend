using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DecalXeAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_V2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryID = table.Column<string>(type: "text", nullable: false),
                    CategoryName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryID);
                });

            migrationBuilder.CreateTable(
                name: "DecalTypes",
                columns: table => new
                {
                    DecalTypeID = table.Column<string>(type: "text", nullable: false),
                    DecalTypeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Material = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Width = table.Column<decimal>(type: "numeric", nullable: true),
                    Height = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecalTypes", x => x.DecalTypeID);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleID = table.Column<string>(type: "text", nullable: false),
                    RoleName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "Stores",
                columns: table => new
                {
                    StoreID = table.Column<string>(type: "text", nullable: false),
                    StoreName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stores", x => x.StoreID);
                });

            migrationBuilder.CreateTable(
                name: "VehicleBrands",
                columns: table => new
                {
                    BrandID = table.Column<string>(type: "text", nullable: false),
                    BrandName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleBrands", x => x.BrandID);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductID = table.Column<string>(type: "text", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false),
                    CategoryID = table.Column<string>(type: "text", nullable: false)
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
                name: "DecalServices",
                columns: table => new
                {
                    ServiceID = table.Column<string>(type: "text", nullable: false),
                    ServiceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    StandardWorkUnits = table.Column<int>(type: "integer", nullable: false),
                    DecalTypeID = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecalServices", x => x.ServiceID);
                    table.ForeignKey(
                        name: "FK_DecalServices_DecalTypes_DecalTypeID",
                        column: x => x.DecalTypeID,
                        principalTable: "DecalTypes",
                        principalColumn: "DecalTypeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DecalTemplates",
                columns: table => new
                {
                    TemplateID = table.Column<string>(type: "text", nullable: false),
                    TemplateName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ImageURL = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DecalTypeID = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecalTemplates", x => x.TemplateID);
                    table.ForeignKey(
                        name: "FK_DecalTemplates_DecalTypes_DecalTypeID",
                        column: x => x.DecalTypeID,
                        principalTable: "DecalTypes",
                        principalColumn: "DecalTypeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    AccountID = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    RoleID = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.AccountID);
                    table.ForeignKey(
                        name: "FK_Accounts_Roles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleModels",
                columns: table => new
                {
                    ModelID = table.Column<string>(type: "text", nullable: false),
                    ModelName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ChassisNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VehicleType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BrandID = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleModels", x => x.ModelID);
                    table.ForeignKey(
                        name: "FK_VehicleModels_VehicleBrands_BrandID",
                        column: x => x.BrandID,
                        principalTable: "VehicleBrands",
                        principalColumn: "BrandID",
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
                name: "Customers",
                columns: table => new
                {
                    CustomerID = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AccountID = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerID);
                    table.ForeignKey(
                        name: "FK_Customers_Accounts_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Accounts",
                        principalColumn: "AccountID");
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeID = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StoreID = table.Column<string>(type: "text", nullable: false),
                    AccountID = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeID);
                    table.ForeignKey(
                        name: "FK_Employees_Accounts_AccountID",
                        column: x => x.AccountID,
                        principalTable: "Accounts",
                        principalColumn: "AccountID");
                    table.ForeignKey(
                        name: "FK_Employees_Stores_StoreID",
                        column: x => x.StoreID,
                        principalTable: "Stores",
                        principalColumn: "StoreID",
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

            migrationBuilder.CreateTable(
                name: "TechLaborPrices",
                columns: table => new
                {
                    ServiceID = table.Column<string>(type: "text", nullable: false),
                    VehicleModelID = table.Column<string>(type: "text", nullable: false),
                    LaborPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechLaborPrices", x => new { x.ServiceID, x.VehicleModelID });
                    table.ForeignKey(
                        name: "FK_TechLaborPrices_DecalServices_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "DecalServices",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TechLaborPrices_VehicleModels_VehicleModelID",
                        column: x => x.VehicleModelID,
                        principalTable: "VehicleModels",
                        principalColumn: "ModelID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleModelDecalTemplates",
                columns: table => new
                {
                    VehicleModelDecalTemplateID = table.Column<string>(type: "text", nullable: false),
                    ModelID = table.Column<string>(type: "text", nullable: false),
                    TemplateID = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleModelDecalTemplates", x => x.VehicleModelDecalTemplateID);
                    table.ForeignKey(
                        name: "FK_VehicleModelDecalTemplates_DecalTemplates_TemplateID",
                        column: x => x.TemplateID,
                        principalTable: "DecalTemplates",
                        principalColumn: "TemplateID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VehicleModelDecalTemplates_VehicleModels_ModelID",
                        column: x => x.ModelID,
                        principalTable: "VehicleModels",
                        principalColumn: "ModelID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerVehicles",
                columns: table => new
                {
                    VehicleID = table.Column<string>(type: "text", nullable: false),
                    ChassisNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                        name: "FK_CustomerVehicles_Customers_CustomerID",
                        column: x => x.CustomerID,
                        principalTable: "Customers",
                        principalColumn: "CustomerID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerVehicles_VehicleModels_ModelID",
                        column: x => x.ModelID,
                        principalTable: "VehicleModels",
                        principalColumn: "ModelID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdminDetails",
                columns: table => new
                {
                    EmployeeID = table.Column<string>(type: "text", nullable: false),
                    AccessLevel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminDetails", x => x.EmployeeID);
                    table.ForeignKey(
                        name: "FK_AdminDetails_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DesignerDetails",
                columns: table => new
                {
                    EmployeeID = table.Column<string>(type: "text", nullable: false),
                    Specialization = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PortfolioUrl = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesignerDetails", x => x.EmployeeID);
                    table.ForeignKey(
                        name: "FK_DesignerDetails_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ManagerDetails",
                columns: table => new
                {
                    EmployeeID = table.Column<string>(type: "text", nullable: false),
                    BudgetManaged = table.Column<decimal>(type: "numeric(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagerDetails", x => x.EmployeeID);
                    table.ForeignKey(
                        name: "FK_ManagerDetails_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesPersonDetails",
                columns: table => new
                {
                    EmployeeID = table.Column<string>(type: "text", nullable: false),
                    CommissionRate = table.Column<decimal>(type: "numeric(5,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesPersonDetails", x => x.EmployeeID);
                    table.ForeignKey(
                        name: "FK_SalesPersonDetails_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TechnicianDetails",
                columns: table => new
                {
                    EmployeeID = table.Column<string>(type: "text", nullable: false),
                    YearsOfExperience = table.Column<int>(type: "integer", nullable: true),
                    Certifications = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnicianDetails", x => x.EmployeeID);
                    table.ForeignKey(
                        name: "FK_TechnicianDetails_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    OrderID = table.Column<string>(type: "text", nullable: false),
                    CustomerID = table.Column<string>(type: "text", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    OrderStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AssignedEmployeeID = table.Column<string>(type: "text", nullable: true),
                    VehicleID = table.Column<string>(type: "text", nullable: true),
                    ExpectedArrivalTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentStage = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsCustomDecal = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderID);
                    table.ForeignKey(
                        name: "FK_Orders_CustomerVehicles_VehicleID",
                        column: x => x.VehicleID,
                        principalTable: "CustomerVehicles",
                        principalColumn: "VehicleID");
                    table.ForeignKey(
                        name: "FK_Orders_Customers_CustomerID",
                        column: x => x.CustomerID,
                        principalTable: "Customers",
                        principalColumn: "CustomerID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Employees_AssignedEmployeeID",
                        column: x => x.AssignedEmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID");
                });

            migrationBuilder.CreateTable(
                name: "CustomServiceRequests",
                columns: table => new
                {
                    CustomRequestID = table.Column<string>(type: "text", nullable: false),
                    CustomerID = table.Column<string>(type: "text", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ReferenceImageURL = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DesiredCompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RequestStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EstimatedCost = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    EstimatedWorkUnits = table.Column<int>(type: "integer", nullable: true),
                    SalesEmployeeID = table.Column<string>(type: "text", nullable: true),
                    OrderID = table.Column<string>(type: "text", nullable: true)
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
                name: "Deposits",
                columns: table => new
                {
                    DepositID = table.Column<string>(type: "text", nullable: false),
                    OrderID = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DepositDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deposits", x => x.DepositID);
                    table.ForeignKey(
                        name: "FK_Deposits_Orders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Designs",
                columns: table => new
                {
                    DesignID = table.Column<string>(type: "text", nullable: false),
                    DesignURL = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DesignerID = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ApprovalStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsAIGenerated = table.Column<bool>(type: "boolean", nullable: false),
                    AIModelUsed = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DesignPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    OrderID = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Designs", x => x.DesignID);
                    table.ForeignKey(
                        name: "FK_Designs_Employees_DesignerID",
                        column: x => x.DesignerID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID");
                    table.ForeignKey(
                        name: "FK_Designs_Orders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID");
                });

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    FeedbackID = table.Column<string>(type: "text", nullable: false),
                    OrderID = table.Column<string>(type: "text", nullable: false),
                    CustomerID = table.Column<string>(type: "text", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FeedbackDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.FeedbackID);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Customers_CustomerID",
                        column: x => x.CustomerID,
                        principalTable: "Customers",
                        principalColumn: "CustomerID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Orders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderDetails",
                columns: table => new
                {
                    OrderDetailID = table.Column<string>(type: "text", nullable: false),
                    OrderID = table.Column<string>(type: "text", nullable: false),
                    ServiceID = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ActualAreaUsed = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ActualLengthUsed = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ActualWidthUsed = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    FinalCalculatedPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetails", x => x.OrderDetailID);
                    table.ForeignKey(
                        name: "FK_OrderDetails_DecalServices_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "DecalServices",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDetails_Orders_OrderID",
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
                name: "Payments",
                columns: table => new
                {
                    PaymentID = table.Column<string>(type: "text", nullable: false),
                    OrderID = table.Column<string>(type: "text", nullable: false),
                    PaymentAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TransactionCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PaymentStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AccountNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PayerName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentID);
                    table.ForeignKey(
                        name: "FK_Payments_Orders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Warranties",
                columns: table => new
                {
                    WarrantyID = table.Column<string>(type: "text", nullable: false),
                    VehicleID = table.Column<string>(type: "text", nullable: false),
                    WarrantyStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WarrantyEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WarrantyType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WarrantyStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OrderID = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warranties", x => x.WarrantyID);
                    table.ForeignKey(
                        name: "FK_Warranties_CustomerVehicles_VehicleID",
                        column: x => x.VehicleID,
                        principalTable: "CustomerVehicles",
                        principalColumn: "VehicleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Warranties_Orders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID");
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
                name: "DesignWorkOrders",
                columns: table => new
                {
                    WorkOrderID = table.Column<string>(type: "text", nullable: false),
                    DesignID = table.Column<string>(type: "text", nullable: false),
                    OrderID = table.Column<string>(type: "text", nullable: false),
                    EstimatedHours = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ActualHours = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Cost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Requirements = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesignWorkOrders", x => x.WorkOrderID);
                    table.ForeignKey(
                        name: "FK_DesignWorkOrders_Designs_DesignID",
                        column: x => x.DesignID,
                        principalTable: "Designs",
                        principalColumn: "DesignID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DesignWorkOrders_Orders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_RoleID",
                table: "Accounts",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_AccountID",
                table: "Customers",
                column: "AccountID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerVehicles_CustomerID",
                table: "CustomerVehicles",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerVehicles_ModelID",
                table: "CustomerVehicles",
                column: "ModelID");

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
                name: "IX_DecalServices_DecalTypeID",
                table: "DecalServices",
                column: "DecalTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_DecalTemplates_DecalTypeID",
                table: "DecalTemplates",
                column: "DecalTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Deposits_OrderID",
                table: "Deposits",
                column: "OrderID");

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
                name: "IX_Designs_DesignerID",
                table: "Designs",
                column: "DesignerID");

            migrationBuilder.CreateIndex(
                name: "IX_Designs_OrderID",
                table: "Designs",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_DesignWorkOrders_DesignID",
                table: "DesignWorkOrders",
                column: "DesignID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DesignWorkOrders_OrderID",
                table: "DesignWorkOrders",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_AccountID",
                table: "Employees",
                column: "AccountID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_StoreID",
                table: "Employees",
                column: "StoreID");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_CustomerID",
                table: "Feedbacks",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_OrderID",
                table: "Feedbacks",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OrderID",
                table: "OrderDetails",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_ServiceID",
                table: "OrderDetails",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_AssignedEmployeeID",
                table: "Orders",
                column: "AssignedEmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerID",
                table: "Orders",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_VehicleID",
                table: "Orders",
                column: "VehicleID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStageHistories_ChangedByEmployeeID",
                table: "OrderStageHistories",
                column: "ChangedByEmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStageHistories_OrderID",
                table: "OrderStageHistories",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderID",
                table: "Payments",
                column: "OrderID");

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

            migrationBuilder.CreateIndex(
                name: "IX_TechLaborPrices_VehicleModelID",
                table: "TechLaborPrices",
                column: "VehicleModelID");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleModelDecalTemplates_ModelID",
                table: "VehicleModelDecalTemplates",
                column: "ModelID");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleModelDecalTemplates_TemplateID",
                table: "VehicleModelDecalTemplates",
                column: "TemplateID");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleModels_BrandID",
                table: "VehicleModels",
                column: "BrandID");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_OrderID",
                table: "Warranties",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_VehicleID",
                table: "Warranties",
                column: "VehicleID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminDetails");

            migrationBuilder.DropTable(
                name: "CustomServiceRequests");

            migrationBuilder.DropTable(
                name: "Deposits");

            migrationBuilder.DropTable(
                name: "DesignComments");

            migrationBuilder.DropTable(
                name: "DesignerDetails");

            migrationBuilder.DropTable(
                name: "DesignWorkOrders");

            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "ManagerDetails");

            migrationBuilder.DropTable(
                name: "OrderDetails");

            migrationBuilder.DropTable(
                name: "OrderStageHistories");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "PrintingPriceDetails");

            migrationBuilder.DropTable(
                name: "SalesPersonDetails");

            migrationBuilder.DropTable(
                name: "ServiceVehicleModelProducts");

            migrationBuilder.DropTable(
                name: "TechLaborPrices");

            migrationBuilder.DropTable(
                name: "TechnicianDetails");

            migrationBuilder.DropTable(
                name: "VehicleModelDecalTemplates");

            migrationBuilder.DropTable(
                name: "Warranties");

            migrationBuilder.DropTable(
                name: "Designs");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "DecalServices");

            migrationBuilder.DropTable(
                name: "DecalTemplates");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "DecalTypes");

            migrationBuilder.DropTable(
                name: "CustomerVehicles");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "VehicleModels");

            migrationBuilder.DropTable(
                name: "Stores");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "VehicleBrands");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
