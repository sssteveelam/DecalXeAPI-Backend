using DecalXeAPI.Data; // Thêm dòng này để import namespace Data
using Npgsql.EntityFrameworkCore.PostgreSQL; // <-- THÊM DÒNG NÀY VÀO ĐÂY
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.MappingProfiles;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAutoMapper(typeof(MainMappingProfile).Assembly); // <-- SỬA DÒNG NÀY

// Đăng ký AutoMapper
 // Dòng này sẽ tự động tìm tất cả các Profile trong cùng Assembly với Program.cs
// Hoặc cụ thể hơn nếu đệ có nhiều Profile rải rác:
// builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
// Huynh sẽ dùng cách AddAutoMapper(typeof(Program)) vì nó đơn giản và đủ dùng.


// Thêm DbContext vào Services container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")) // Sử dụng Npgsql cho PostgreSQL
);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
