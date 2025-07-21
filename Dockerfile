# Giai đoạn 1: Build ứng dụng
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
# Đã sửa: không còn "DecalXeAPI/" ở trước nữa
RUN dotnet restore "DecalXeAPI.csproj"
RUN dotnet publish "DecalXeAPI.csproj" -c Release -o /app/publish

# Giai đoạn 2: Chạy ứng dụng
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DecalXeAPI.dll"]