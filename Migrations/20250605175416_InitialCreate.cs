using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DecalXeAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "Products",
                columns: table => new
                {
                    ProductID = table.Column<string>(type: "text", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductID);
                });

            migrationBuilder.CreateTable(
                name: "Promotions",
                columns: table => new
                {
                    PromotionID = table.Column<string>(type: "text", nullable: false),
                    PromotionName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promotions", x => x.PromotionID);
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
                name: "TimeSlotDefinitions",
                columns: table => new
                {
                    SlotDefID = table.Column<string>(type: "text", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeSlotDefinitions", x => x.SlotDefID);
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
                name: "ServiceProducts",
                columns: table => new
                {
                    ServiceProductID = table.Column<string>(type: "text", nullable: false),
                    ServiceID = table.Column<string>(type: "text", nullable: false),
                    DecalServiceServiceID = table.Column<string>(type: "text", nullable: true),
                    ProductID = table.Column<string>(type: "text", nullable: false),
                    QuantityUsed = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceProducts", x => x.ServiceProductID);
                    table.ForeignKey(
                        name: "FK_ServiceProducts_DecalServices_DecalServiceServiceID",
                        column: x => x.DecalServiceServiceID,
                        principalTable: "DecalServices",
                        principalColumn: "ServiceID");
                    table.ForeignKey(
                        name: "FK_ServiceProducts_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceDecalTemplates",
                columns: table => new
                {
                    ServiceDecalTemplateID = table.Column<string>(type: "text", nullable: false),
                    ServiceID = table.Column<string>(type: "text", nullable: false),
                    DecalServiceServiceID = table.Column<string>(type: "text", nullable: true),
                    TemplateID = table.Column<string>(type: "text", nullable: false),
                    DecalTemplateTemplateID = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceDecalTemplates", x => x.ServiceDecalTemplateID);
                    table.ForeignKey(
                        name: "FK_ServiceDecalTemplates_DecalServices_DecalServiceServiceID",
                        column: x => x.DecalServiceServiceID,
                        principalTable: "DecalServices",
                        principalColumn: "ServiceID");
                    table.ForeignKey(
                        name: "FK_ServiceDecalTemplates_DecalTemplates_DecalTemplateTemplateID",
                        column: x => x.DecalTemplateTemplateID,
                        principalTable: "DecalTemplates",
                        principalColumn: "TemplateID");
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
                name: "Orders",
                columns: table => new
                {
                    OrderID = table.Column<string>(type: "text", nullable: false),
                    CustomerID = table.Column<string>(type: "text", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    OrderStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AssignedEmployeeID = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.OrderID);
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
                name: "TechnicianDailySchedules",
                columns: table => new
                {
                    DailyScheduleID = table.Column<string>(type: "text", nullable: false),
                    EmployeeID = table.Column<string>(type: "text", nullable: false),
                    ScheduleDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalAvailableWorkUnits = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnicianDailySchedules", x => x.DailyScheduleID);
                    table.ForeignKey(
                        name: "FK_TechnicianDailySchedules_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
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
                name: "Designs",
                columns: table => new
                {
                    DesignID = table.Column<string>(type: "text", nullable: false),
                    OrderID = table.Column<string>(type: "text", nullable: false),
                    DesignURL = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DesignerID = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ApprovalStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsAIGenerated = table.Column<bool>(type: "boolean", nullable: false),
                    AIModelUsed = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AIPrompt = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
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
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Cascade);
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
                    DecalServiceServiceID = table.Column<string>(type: "text", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetails", x => x.OrderDetailID);
                    table.ForeignKey(
                        name: "FK_OrderDetails_DecalServices_DecalServiceServiceID",
                        column: x => x.DecalServiceServiceID,
                        principalTable: "DecalServices",
                        principalColumn: "ServiceID");
                    table.ForeignKey(
                        name: "FK_OrderDetails_Orders_OrderID",
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
                    PromotionID = table.Column<string>(type: "text", nullable: true),
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
                    table.ForeignKey(
                        name: "FK_Payments_Promotions_PromotionID",
                        column: x => x.PromotionID,
                        principalTable: "Promotions",
                        principalColumn: "PromotionID");
                });

            migrationBuilder.CreateTable(
                name: "Warranties",
                columns: table => new
                {
                    WarrantyID = table.Column<string>(type: "text", nullable: false),
                    OrderID = table.Column<string>(type: "text", nullable: false),
                    WarrantyStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WarrantyEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WarrantyType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WarrantyStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warranties", x => x.WarrantyID);
                    table.ForeignKey(
                        name: "FK_Warranties_Orders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledWorkUnits",
                columns: table => new
                {
                    ScheduledWorkUnitID = table.Column<string>(type: "text", nullable: false),
                    DailyScheduleID = table.Column<string>(type: "text", nullable: false),
                    SlotDefID = table.Column<string>(type: "text", nullable: false),
                    TimeSlotDefinitionSlotDefID = table.Column<string>(type: "text", nullable: true),
                    OrderID = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledWorkUnits", x => x.ScheduledWorkUnitID);
                    table.ForeignKey(
                        name: "FK_ScheduledWorkUnits_Orders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "Orders",
                        principalColumn: "OrderID");
                    table.ForeignKey(
                        name: "FK_ScheduledWorkUnits_TechnicianDailySchedules_DailyScheduleID",
                        column: x => x.DailyScheduleID,
                        principalTable: "TechnicianDailySchedules",
                        principalColumn: "DailyScheduleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduledWorkUnits_TimeSlotDefinitions_TimeSlotDefinitionSl~",
                        column: x => x.TimeSlotDefinitionSlotDefID,
                        principalTable: "TimeSlotDefinitions",
                        principalColumn: "SlotDefID");
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
                name: "IX_Designs_DesignerID",
                table: "Designs",
                column: "DesignerID");

            migrationBuilder.CreateIndex(
                name: "IX_Designs_OrderID",
                table: "Designs",
                column: "OrderID",
                unique: true);

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
                column: "OrderID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_DecalServiceServiceID",
                table: "OrderDetails",
                column: "DecalServiceServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OrderID",
                table: "OrderDetails",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_AssignedEmployeeID",
                table: "Orders",
                column: "AssignedEmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerID",
                table: "Orders",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderID",
                table: "Payments",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PromotionID",
                table: "Payments",
                column: "PromotionID");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledWorkUnits_DailyScheduleID",
                table: "ScheduledWorkUnits",
                column: "DailyScheduleID");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledWorkUnits_OrderID",
                table: "ScheduledWorkUnits",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledWorkUnits_TimeSlotDefinitionSlotDefID",
                table: "ScheduledWorkUnits",
                column: "TimeSlotDefinitionSlotDefID");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceDecalTemplates_DecalServiceServiceID",
                table: "ServiceDecalTemplates",
                column: "DecalServiceServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceDecalTemplates_DecalTemplateTemplateID",
                table: "ServiceDecalTemplates",
                column: "DecalTemplateTemplateID");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceProducts_DecalServiceServiceID",
                table: "ServiceProducts",
                column: "DecalServiceServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceProducts_ProductID",
                table: "ServiceProducts",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicianDailySchedules_EmployeeID",
                table: "TechnicianDailySchedules",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_Warranties_OrderID",
                table: "Warranties",
                column: "OrderID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomServiceRequests");

            migrationBuilder.DropTable(
                name: "Designs");

            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "OrderDetails");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "ScheduledWorkUnits");

            migrationBuilder.DropTable(
                name: "ServiceDecalTemplates");

            migrationBuilder.DropTable(
                name: "ServiceProducts");

            migrationBuilder.DropTable(
                name: "Warranties");

            migrationBuilder.DropTable(
                name: "Promotions");

            migrationBuilder.DropTable(
                name: "TechnicianDailySchedules");

            migrationBuilder.DropTable(
                name: "TimeSlotDefinitions");

            migrationBuilder.DropTable(
                name: "DecalTemplates");

            migrationBuilder.DropTable(
                name: "DecalServices");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "DecalTypes");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Stores");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
