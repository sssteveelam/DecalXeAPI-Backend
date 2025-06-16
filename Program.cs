using DecalXeAPI.Data;
using DecalXeAPI.MappingProfiles;
using DecalXeAPI.Middleware;
using DecalXeAPI.Services.Interfaces;
using DecalXeAPI.Services.Implementations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Npgsql; // Cần cho việc xử lý Connection String của Railway
using System; // Cần cho Uri và Environment
using System.Linq; // Cần cho Linq (ví dụ: .Last() cho URI segments)


var builder = WebApplication.CreateBuilder(args);

// --- CẤU HÌNH CÁC DỊCH VỤ (SERVICES) ---

// 1. Cấu hình DbContext (Entity Framework Core)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    string? connectionString; // <-- KHAI BÁO BIẾN CÓ PHẠM VI RỘNG HƠN
    string? railwayDatabaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL"); // <-- KHAI BÁO BIẾN CÓ PHẠM VI RỘNG HƠN

    // Nếu có DATABASE_URL (ví dụ: chạy trên Railway), thì ưu tiên dùng nó
    if (!string.IsNullOrEmpty(railwayDatabaseUrl))
    {
        try
        {
            // Phân tích URL của Railway để xây dựng Connection String cho Npgsql
            var uri = new Uri(railwayDatabaseUrl);
            var userInfo = uri.UserInfo.Split(':'); // Tách username và password
            var host = uri.Host;
            var port = uri.Port;
            var username = userInfo[0];
            var password = userInfo[1];
            var database = uri.Segments.Last(); // Lấy tên database từ cuối URL Path

            // Xây dựng lại Connection String theo định dạng Npgsql
            connectionString = $"Host={host};Port={port};Username={username};Password={password};Database={database};Ssl Mode=Require;Trust Server Certificate=true";
        }
        catch (Exception ex)
        {
            // Ném lỗi nếu không thể phân tích DATABASE_URL
            throw new InvalidOperationException($"Không thể phân tích biến môi trường DATABASE_URL: {railwayDatabaseUrl}. Chi tiết: {ex.Message}");
        }
    }
    else
    {
        // Nếu không có DATABASE_URL (ví dụ: chạy local), thì lấy từ appsettings.json
        connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    }

    // Nếu connectionString vẫn rỗng hoặc null sau khi thử cả hai cách, ném lỗi
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
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderDetailService, OrderDetailService>();
builder.Services.AddScoped<ICustomServiceRequestService, CustomServiceRequestService>();
builder.Services.AddScoped<IDesignService, DesignService>();
builder.Services.AddScoped<ITechnicianDailyScheduleService, TechnicianDailyScheduleService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IStoreService, StoreService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IDecalTypeService, DecalTypeService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<IServiceProductService, ServiceProductService>();
builder.Services.AddScoped<IDecalTemplateService, DecalTemplateService>();
builder.Services.AddScoped<IServiceDecalTemplateService, ServiceDecalTemplateService>();
builder.Services.AddScoped<ITimeSlotDefinitionService, TimeSlotDefinitionService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<IWarrantyService, WarrantyService>();
builder.Services.AddScoped<IPrintingPriceDetailService, PrintingPriceDetailService>();
builder.Services.AddScoped<IDesignCommentService, DesignCommentService>();
builder.Services.AddScoped<IOrderCompletionImageService, OrderCompletionImageService>();


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
        policy => policy.WithOrigins("http://localhost:3000", "http://localhost:3001") // Cho phép Frontend cục bộ
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
});


var app = builder.Build();

// --- CẤU HÌNH CÁC MIDDLEWARE (PIPELINE XỬ LÝ REQUEST) ---

// 1. Global Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 2. Chuyển hướng HTTP sang HTTPS
// app.UseHttpsRedirection(); // Mặc định tắt, có thể bật nếu cần

// 3. Swagger UI (Chỉ dùng trong môi trường Phát triển)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 4. Sử dụng CORS
app.UseCors("AllowSpecificOrigin");

// 5. Sử dụng Authentication và Authorization
app.UseAuthentication();
app.UseAuthorization();

// 6. Map các Controller
app.MapControllers();

// Khởi chạy ứng dụng
app.Run();