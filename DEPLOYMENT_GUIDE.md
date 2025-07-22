# Hướng dẫn Deploy lên Supabase và Render

## Tổng quan
- **Database**: Đã được cấu hình với Supabase PostgreSQL
- **API**: Sẽ được deploy lên Render sử dụng Docker

## Bước 1: Chuẩn bị Repository

1. Đảm bảo code đã được push lên GitHub repository
2. Repository phải có quyền public hoặc bạn phải có quyền truy cập

## Bước 2: Deploy lên Render

### Cách 1: Sử dụng Render Dashboard (Khuyến nghị)

1. Truy cập [https://render.com](https://render.com)
2. Đăng nhập hoặc tạo tài khoản mới
3. Click "New +" → "Web Service"
4. Kết nối với GitHub repository của bạn
5. Cấu hình deployment:
   - **Name**: `decalxe-api`
   - **Region**: Singapore (hoặc region gần nhất)
   - **Branch**: `main`
   - **Runtime**: `Docker`
   - **Build Command**: `docker build -t decalxe-api .`
   - **Start Command**: `dotnet DecalXeAPI.dll --urls http://0.0.0.0:10000`

6. Thêm Environment Variables:
   ```
   ASPNETCORE_ENVIRONMENT = Production
   ASPNETCORE_URLS = http://0.0.0.0:10000
   DATABASE_URL = Host=db.ggvnxocvdcikktrbllvk.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=rgQfHH7+8v6FR!+;Ssl Mode=Require;Trust Server Certificate=true
   ```

7. Click "Create Web Service"

### Cách 2: Sử dụng render.yaml (Tự động)

1. File `render.yaml` đã được tạo trong project
2. Push code lên GitHub
3. Trong Render Dashboard, chọn "New +" → "Blueprint"
4. Kết nối repository và Render sẽ tự động đọc cấu hình từ `render.yaml`

## Bước 3: Kiểm tra Deployment

1. Sau khi deploy thành công, bạn sẽ nhận được URL như: `https://decalxe-api.onrender.com`
2. Truy cập `https://decalxe-api.onrender.com/swagger` để kiểm tra API documentation
3. Test các endpoint để đảm bảo kết nối database hoạt động

## Bước 4: Cập nhật CORS (nếu cần)

Nếu bạn có frontend, cập nhật CORS trong `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", 
        policy => policy.WithOrigins(
                        "https://your-frontend-domain.com",
                        "http://localhost:3000",
                        "http://localhost:3001"
                    )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
});
```

## Lưu ý quan trọng

1. **Free Tier Limitations**: Render free tier có giới hạn:
   - Service sẽ "sleep" sau 15 phút không hoạt động
   - 750 giờ/tháng (khoảng 31 ngày)
   - Băng thông 100GB/tháng

2. **Database**: Supabase đã được cấu hình và sẵn sàng sử dụng

3. **SSL**: Render tự động cung cấp SSL certificate

4. **Monitoring**: Kiểm tra logs trong Render Dashboard nếu có lỗi

## Troubleshooting

### Lỗi kết nối Database
- Kiểm tra connection string trong environment variables
- Đảm bảo Supabase database đang hoạt động

### Service không start
- Kiểm tra logs trong Render Dashboard
- Đảm bảo port 10000 được expose trong Dockerfile

### CORS errors
- Cập nhật CORS policy trong Program.cs
- Thêm domain frontend vào AllowedOrigins
