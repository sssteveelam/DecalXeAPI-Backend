using DecalXeAPI.Data;
using DecalXeAPI.MappingProfiles;
using DecalXeAPI.Middleware;
using DecalXeAPI.Services.Interfaces;
using DecalXeAPI.Services.Implementations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore; // Cần cho context.Database.Migrate()
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Npgsql;
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection; // Cần cho CreateScope(), GetRequiredService<T>()
using Microsoft.Extensions.Logging; // Cần cho ILogger trong khối Migration
using Swashbuckle.AspNetCore.Filters; // <-- THÊM DÒNG NÀY

var builder = WebApplication.CreateBuilder(args);

// --- CẤU HÌNH CÁC DỊCH VỤ (SERVICES) ---

// 1. Cấu hình DbContext (Entity Framework Core)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    string? connectionString;
    string? railwayDatabaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    Console.WriteLine($"DATABASE_URL @tuantu: {railwayDatabaseUrl}");

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
        Console.WriteLine($"DefaultConnection: {connectionString}");
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
// --- Đăng ký Service Layer ---
// Các Service cũ đã được xóa bỏ. Đây là danh sách các Service mới và còn lại.
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IStoreService, StoreService>();

builder.Services.AddScoped<IDecalTypeService, DecalTypeService>();
builder.Services.AddScoped<IDecalTemplateService, DecalTemplateService>();

builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderDetailService, OrderDetailService>();
builder.Services.AddScoped<IOrderStageHistoryService, OrderStageHistoryService>();
builder.Services.AddScoped<IDesignService, DesignService>();
builder.Services.AddScoped<IDesignTemplateItemService, DesignTemplateItemService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<IWarrantyService, WarrantyService>();
builder.Services.AddScoped<IDesignCommentService, DesignCommentService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Các Service cho Vehicle
builder.Services.AddScoped<IVehicleBrandService, VehicleBrandService>();
builder.Services.AddScoped<IVehicleModelService, VehicleModelService>();
builder.Services.AddScoped<ICustomerVehicleService, CustomerVehicleService>();



// Các Service cho các bảng mới
builder.Services.AddScoped<IDepositService, DepositService>();
builder.Services.AddScoped<IDesignWorkOrderService, DesignWorkOrderService>();
builder.Services.AddScoped<ITechLaborPriceService, TechLaborPriceService>();

// 4. Cấu hình Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DecalXeAPI", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập 'Bearer ' + JWT Token của bạn. Ví dụ: 'Bearer eyJhbGciOiJIUzI1Ni...'",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 5. Cấu hình Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key không được cấu hình.")))
    };
});

// 6. Thêm Authorization Policy
builder.Services.AddAuthorization();

// 7. Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy => policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
});


// 7. Cấu hình CORS (Cross-Origin Resource Sharing)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", 
        policy => policy.AllowAnyOrigin() 
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        );
});


var app = builder.Build(); // <-- app được Build ở đây




using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate(); // <-- Lệnh chạy tất cả migrations chưa được áp dụng
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Đã xảy ra lỗi khi di chuyển database.");
    }
}
// --- KẾT THÚC PHẦN TỰ ĐỘNG CHẠY MIGRATION ---


// --- CẤU HÌNH CÁC MIDDLEWARE (PIPELINE XỬ LÝ REQUEST) ---
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 2. Chuyển hướng HTTP sang HTTPS
// app.UseHttpsRedirection();

// 3. Swagger UI (Chỉ dùng trong môi trường Phát triển)
app.UseSwagger();
app.UseSwaggerUI(options => { options.SwaggerEndpoint("/swagger/v1/swagger.json", "DecalXeAPI v1"); });

// 4. Sử dụng CORS
app.UseCors("AllowSpecificOrigin");

// 5. Sử dụng Authentication và Authorization
app.UseAuthentication();
app.UseAuthorization();

// 6. Map các Controller
app.MapControllers();

// Khởi chạy ứng dụng
app.Run();