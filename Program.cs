using DecalXeAPI.Data;
using DecalXeAPI.MappingProfiles;
using DecalXeAPI.Middleware;
using DecalXeAPI.Models;
using DecalXeAPI.QueryParams;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces; // Cần cho các Interface Services
using DecalXeAPI.Services.Implementations; // Cần cho các Implementation Services
// Thêm using cho VehicleService nếu nó nằm ở namespace khác
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Npgsql;
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


var builder = WebApplication.CreateBuilder(args);

// --- CẤU HÌNH CÁC DỊCH VỤ (SERVICES) ---

// 1. Cấu hình DbContext (Entity Framework Core)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    string? connectionString;
    string? railwayDatabaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

    if (!string.IsNullOrEmpty(railwayDatabaseUrl))
    {
        try
        {
            var uri = new Uri(railwayDatabaseUrl);
            var userInfo = uri.UserInfo.Split(':');
            var host = uri.Host;
            var port = uri.Port;
            var username = userInfo[0];
            var password = userInfo[1];
            var database = uri.Segments.Last();

            connectionString = $"Host={host};Port={port};Username={username};Password={password};Database={database};Ssl Mode=Require;Trust Server Certificate=true";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Không thể phân tích biến môi trường DATABASE_URL: {railwayDatabaseUrl}. Chi tiết: {ex.Message}");
        }
    }
    else
    {
        connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    }

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Connection string 'DefaultConnection' hoặc biến môi trường 'DATABASE_URL' không được cấu hình.");
    }

    options.UseNpgsql(connectionString);
});

// 2. Cấu hình AutoMapper
builder.Services.AddAutoMapper(typeof(MainMappingProfile).Assembly);

// 3. Thêm Controllers
builder.Services.AddControllers();

// --- Đăng ký Service Layer ---
// Chỉ giữ lại các Services chắc chắn còn tồn tại trong project
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderDetailService, OrderDetailService>();
builder.Services.AddScoped<ICustomServiceRequestService, CustomServiceRequestService>();
builder.Services.AddScoped<IDesignService, DesignService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IStoreService, StoreService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IDecalTypeService, DecalTypeService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IDecalTemplateService, DecalTemplateService>();
builder.Services.AddScoped<IServiceDecalTemplateService, ServiceDecalTemplateService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<IWarrantyService, WarrantyService>();
builder.Services.AddScoped<IPrintingPriceDetailService, PrintingPriceDetailService>();
builder.Services.AddScoped<IDesignCommentService, DesignCommentService>();

// MỚI TỪ REVIEW2: Đăng ký các Services mới (chưa tạo Services implementation)
// builder.Services.AddScoped<IAdminDetailService, AdminDetailService>();
// builder.Services.AddScoped<IManagerDetailService, ManagerDetailService>();
// builder.Services.AddScoped<ISalesPersonDetailService, SalesPersonDetailService>();
// builder.Services.AddScoped<IDesignerDetailService, DesignerDetailService>();
// builder.Services.AddScoped<ITechnicianDetailService, TechnicianDetailService>();
// builder.Services.AddScoped<IDepositService, DepositService>();
// builder.Services.AddScoped<ITechLaborPriceService, TechLaborPriceService>();
// builder.Services.AddScoped<IDesignWorkOrderService, DesignWorkOrderService>();
// builder.Services.AddScoped<IServiceVehicleModelProductService, ServiceVehicleModelProductService>();

// Các Services liên quan đến Vehicle và CustomerVehicle
builder.Services.AddScoped<IVehicleService, VehicleService>(); // <-- MỚI THÊM
builder.Services.AddScoped<ICustomerVehicleService, CustomerVehicleService>(); // <-- MỚI THÊM

var app = builder.Build();

// --- TỰ ĐỘNG CHẠY MIGRATION KHI ỨNG DỤNG KHỞI ĐỘNG ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Đã xảy ra lỗi khi di chuyển database.");
    }
}

// --- CẤU HÌNH CÁC MIDDLEWARE (PIPELINE XỬ LÝ REQUEST) ---
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>{options.SwaggerEndpoint("/swagger/v1/swagger.json", "DecalXeAPI v1");});

app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();