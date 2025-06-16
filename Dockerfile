# Giai đoạn Build: Sử dụng .NET SDK 8.0 để build ứng dụng
# Đây là base image chứa .NET 8.0 SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy các file .csproj và khôi phục các gói NuGet
COPY ["DecalXeAPI.csproj", "."]
RUN dotnet restore

# Copy toàn bộ mã nguồn của ứng dụng
COPY . .

# Build ứng dụng ở chế độ Release
# --no-restore: không chạy dotnet restore lại (đã làm ở trên)
# -o /app/build: xuất bản kết quả build vào thư mục /app/build
RUN dotnet build "DecalXeAPI.csproj" -c Release -o /app/build

# Giai đoạn Publish: Tạo bản phân phối cuối cùng
# Sử dụng runtime-image nhẹ hơn chỉ chứa môi trường chạy .NET 8.0
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS publish
WORKDIR /app

# Copy kết quả build từ giai đoạn 'build' sang giai đoạn 'publish'
COPY --from=build /app/build .

# Định nghĩa cổng mà ứng dụng sẽ lắng nghe
# Mặc định của ASP.NET Core là 8080 cho HTTP và 8081 cho HTTPS
# Railway sẽ map cổng public của nó vào cổng này
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Lệnh chạy ứng dụng khi container khởi động
# Tên DLL sẽ là tên project của bạn (ví dụ: DecalXeAPI.dll)
ENTRYPOINT ["dotnet", "DecalXeAPI.dll"]