# ✅ Checklist Deploy lên Render

## Chuẩn bị (Đã hoàn thành)
- [x] Cấu hình Dockerfile với port 10000
- [x] Tạo render.yaml cho tự động deploy
- [x] Cấu hình appsettings.Production.json
- [x] Tạo .dockerignore để tối ưu build
- [x] Database Supabase đã sẵn sàng

## Bước deploy trên Render.com

### Option 1: Manual Deploy
1. [ ] Truy cập https://render.com
2. [ ] Đăng nhập/Đăng ký tài khoản
3. [ ] Click "New +" → "Web Service"
4. [ ] Connect GitHub repository
5. [ ] Cấu hình:
   - Name: `decalxe-api`
   - Region: Singapore
   - Branch: `main`
   - Runtime: `Docker`
6. [ ] Thêm Environment Variables:
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://0.0.0.0:10000
   ```
7. [ ] Click "Create Web Service"

### Option 2: Blueprint Deploy (Khuyến nghị)
1. [ ] Push code lên GitHub
2. [ ] Truy cập https://render.com
3. [ ] Click "New +" → "Blueprint"
4. [ ] Connect repository
5. [ ] Render sẽ tự động đọc render.yaml

## Sau khi deploy
1. [ ] Kiểm tra URL: `https://your-service-name.onrender.com`
2. [ ] Test Swagger UI: `https://your-service-name.onrender.com/swagger`
3. [ ] Test API endpoints
4. [ ] Kiểm tra logs nếu có lỗi

## URLs quan trọng
- **Render Dashboard**: https://dashboard.render.com
- **Supabase Dashboard**: https://supabase.com/dashboard
- **API Documentation**: https://your-service-name.onrender.com/swagger

## Lưu ý
- Free tier Render có giới hạn 750h/tháng
- Service sẽ "ngủ" sau 15 phút không hoạt động
- Cold start có thể mất 30-60 giây
