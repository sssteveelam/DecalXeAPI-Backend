using DecalXeAPI.Data;
using DecalXeAPI.MappingProfiles;
using DecalXeAPI.Middleware;
using DecalXeAPI.Models;
using DecalXeAPI.QueryParams;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces; // Cần cho các Interface Services
using DecalXeAPI.Services.Implementations; // Cần cho các Implementation Services
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; // Cần cho OpenApiInfo, OpenApiSecurityScheme, v.v.
using System.Text;
using Npgsql; // Cần cho việc xử lý Connection String của Railway
using System; // Cần cho Uri và Environment
using System.Linq; // Cần cho Linq (ví dụ: .Last() cho URI segments)
using Microsoft.Extensions.DependencyInjection; // Cần cho CreateScope(), GetRequiredService<T>()
using Microsoft.Extensions.Logging; // Cần cho ILogger trong khối Migration
using System.Reflection; // Cần cho Assembly.GetExecutingAssembly()
using System.IO; // Cần cho Path.Combine()


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

// Các Services liên quan đến Vehicle và CustomerVehicle (đã được comment/xóa để quay về trạng thái ổn định)
// builder.Services.AddScoped<IVehicleService, VehicleService>();
// builder.Services.AddScoped<ICustomerVehicleService, CustomerVehicleService>();

// Các Services mới từ Review2 (đã được comment để quay về trạng thái ổn định)
// builder.Services.AddScoped<IAdminDetailService, AdminDetailService>();
// builder.Services.AddScoped<IManagerDetailService, ManagerDetailService>();
// builder.Services.AddScoped<ISalesPersonDetailService, SalesPersonDetailService>();
// builder.Services.AddScoped<IDesignerDetailService, DesignerDetailService>();
// builder.Services.AddScoped<ITechnicianDetailService, TechnicianDetailService>();
// builder.Services.AddScoped<IDepositService, DepositService>();
// builder.Services.AddScoped<ITechLaborPriceService, TechLaborPriceService>();
// builder.Services.AddScoped<IDesignWorkOrderService, DesignWorkOrderService>();
// builder.Services.AddScoped<IServiceVehicleModelProductService, ServiceVehicleModelProductService>();


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

    // Đảm bảo dòng này có để đọc XML Comments từ project
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
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
        policy => policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        );
});


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

// Swagger UI (Chỉ dùng trong môi trường Phát triển)
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "DecalXeAPI v1");
    });

app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
