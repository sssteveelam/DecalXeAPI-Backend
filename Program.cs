using DecalXeAPI.Data; // Thêm dòng này để import namespace Data
using Npgsql.EntityFrameworkCore.PostgreSQL; // <-- THÊM DÒNG NÀY VÀO ĐÂY
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


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
