using DecalXeAPI.Data;
using DecalXeAPI.MappingProfiles;
using DecalXeAPI.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; // Cần cho cấu hình Swagger
using System.Text;
using DecalXeAPI.Services.Interfaces;
using DecalXeAPI.Services.Implementations;


var builder = WebApplication.CreateBuilder(args);

// --- CẤU HÌNH CÁC DỊCH VỤ (SERVICES) ---
// Các dịch vụ này được đăng ký vào "bộ chứa dịch vụ" (Dependency Injection container)
// để các thành phần khác của ứng dụng có thể sử dụng chúng.

// 1. Cấu hình DbContext (Entity Framework Core)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")) // Sử dụng Npgsql cho PostgreSQL và lấy chuỗi kết nối
);

// 2. Cấu hình AutoMapper
// AutoMapper sẽ tự động tìm tất cả các Mapping Profiles (ví dụ: MainMappingProfile)
// trong cùng assembly với MainMappingProfile.
builder.Services.AddAutoMapper(typeof(MainMappingProfile).Assembly);

// 3. Thêm Controllers (cho các API Controller truyền thống)
builder.Services.AddControllers();


// Đăng ký Service Layer
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
// AddScoped nghĩa là một instance của OrderService sẽ được tạo một lần cho mỗi HTTP request.
// Đây là lifetime phù hợp cho các services tương tác với DbContext.

// 4. Cấu hình Swagger/OpenAPI (để tạo tài liệu API tự động và giao diện test API)
builder.Services.AddEndpointsApiExplorer(); // Cần thiết cho Swagger để khám phá các endpoint
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DecalXeAPI", Version = "v1" });
    // Cấu hình để Swagger UI có thể sử dụng JWT Token (Optional nhưng rất hữu ích để test Authorization)
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

// 5. Cấu hình Authentication (Xác thực người dùng)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, // Xác minh người phát hành token
        ValidateAudience = true, // Xác minh đối tượng nhận token
        ValidateLifetime = true, // Xác minh thời gian sống của token
        ValidateIssuerSigningKey = true, // Xác minh khóa ký token
        ValidIssuer = builder.Configuration["Jwt:Issuer"], // Lấy Issuer từ appsettings.json
        ValidAudience = builder.Configuration["Jwt:Audience"], // Lấy Audience từ appsettings.json
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key không được cấu hình.")))
    };
});

// 6. Thêm Authorization Policy (Phân quyền người dùng đã xác thực)
builder.Services.AddAuthorization();

// 7. Cấu hình CORS (Cross-Origin Resource Sharing)
// Cho phép Frontend (chạy ở domain/port khác) có thể gọi API của Backend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("http://localhost:3000", "http://localhost:3001") // Thay đổi nếu Frontend của đệ chạy ở cổng khác
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials()); // Cho phép gửi cookies, authorization headers, v.v.
});


var app = builder.Build();

// --- CẤU HÌNH CÁC MIDDLEWARE (PIPELINE XỬ LÝ REQUEST) ---
// Thứ tự các Middleware RẤT QUAN TRỌNG!

// 1. Global Exception Handling Middleware (PHẢI ĐỨNG ĐẦU TIÊN để bắt lỗi từ tất cả các middleware khác)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 2. Chuyển hướng HTTP sang HTTPS (nếu muốn dùng HTTPS)
// app.UseHttpsRedirection(); // Đang tắt ở project template --no-https, có thể bật sau

// 3. Swagger UI (Chỉ dùng trong môi trường Phát triển)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Bật Swagger JSON endpoint
    app.UseSwaggerUI(); // Bật giao diện Swagger UI
}

// 4. Sử dụng CORS
app.UseCors("AllowSpecificOrigin");

// 5. Sử dụng Authentication và Authorization
// Authentication phải đứng trước Authorization
app.UseAuthentication();
app.UseAuthorization();

// 6. Map các Controller (định tuyến các request đến đúng Controller và Action)
app.MapControllers();

// Khởi chạy ứng dụng
app.Run();