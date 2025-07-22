# API Endpoints Summary - CustomerVehicles

## Tính năng "Thêm biển số xe" đã được triển khai thành công!

### 🚗 **API Endpoints cho CustomerVehicles**

#### **1. Lấy danh sách tất cả xe**
```
GET /api/CustomerVehicles
```
- **Mô tả**: Lấy danh sách tất cả xe khách hàng
- **Response**: `CustomerVehicleDto[]`

#### **2. Lấy xe theo ID**
```
GET /api/CustomerVehicles/{id}
```
- **Mô tả**: Lấy thông tin xe theo ID
- **Parameters**: `id` (string) - ID của xe
- **Response**: `CustomerVehicleDto`

#### **3. Lấy xe theo biển số** ⭐ **TÍNH NĂNG MỚI**
```
GET /api/CustomerVehicles/by-license-plate/{licensePlate}
```
- **Mô tả**: Lấy thông tin xe theo biển số
- **Parameters**: `licensePlate` (string) - Biển số xe
- **Response**: `CustomerVehicleDto`
- **Ví dụ**: `/api/CustomerVehicles/by-license-plate/51F-12345`

#### **4. Lấy xe theo khách hàng**
```
GET /api/CustomerVehicles/by-customer/{customerId}
```
- **Mô tả**: Lấy danh sách xe của một khách hàng
- **Parameters**: `customerId` (string) - ID khách hàng
- **Response**: `CustomerVehicleDto[]`

#### **5. Tạo xe mới**
```
POST /api/CustomerVehicles
```
- **Mô tả**: Tạo xe mới với biển số
- **Body**: `CreateCustomerVehicleDto`
```json
{
  "chassisNumber": "ABC123456789",
  "licensePlate": "51F-12345",
  "color": "Đỏ",
  "year": 2023,
  "initialKM": 0,
  "customerId": "customer-id-here",
  "modelId": "model-id-here"
}
```
- **Response**: `CustomerVehicleDto`

#### **6. Cập nhật xe**
```
PUT /api/CustomerVehicles/{id}
```
- **Mô tả**: Cập nhật thông tin xe (bao gồm biển số)
- **Parameters**: `id` (string) - ID của xe
- **Body**: `UpdateCustomerVehicleDto`
```json
{
  "chassisNumber": "ABC123456789",
  "licensePlate": "51F-54321",
  "color": "Xanh",
  "year": 2023,
  "initialKM": 1000,
  "modelId": "new-model-id"
}
```
- **Response**: `CustomerVehicleDto`

#### **7. Xóa xe**
```
DELETE /api/CustomerVehicles/{id}
```
- **Mô tả**: Xóa xe
- **Parameters**: `id` (string) - ID của xe
- **Response**: Success message

#### **8. Kiểm tra xe tồn tại**
```
GET /api/CustomerVehicles/{id}/exists
```
- **Mô tả**: Kiểm tra xe có tồn tại không
- **Parameters**: `id` (string) - ID của xe
- **Response**: `boolean`

#### **9. Kiểm tra biển số tồn tại** ⭐ **TÍNH NĂNG MỚI**
```
GET /api/CustomerVehicles/license-plate/{licensePlate}/exists
```
- **Mô tả**: Kiểm tra biển số có tồn tại không
- **Parameters**: `licensePlate` (string) - Biển số xe
- **Response**: `boolean`
- **Ví dụ**: `/api/CustomerVehicles/license-plate/51F-12345/exists`

#### **10. Kiểm tra số khung tồn tại**
```
GET /api/CustomerVehicles/chassis/{chassisNumber}/exists
```
- **Mô tả**: Kiểm tra số khung có tồn tại không
- **Parameters**: `chassisNumber` (string) - Số khung xe
- **Response**: `boolean`

### 📋 **DTOs (Data Transfer Objects)**

#### **CustomerVehicleDto** (Response)
```json
{
  "vehicleID": "string",
  "chassisNumber": "string",
  "licensePlate": "string",    // ⭐ TRƯỜNG MỚI
  "color": "string",
  "year": 2023,
  "initialKM": 0.0,
  "customerID": "string",
  "customerFullName": "string",
  "modelID": "string",
  "vehicleModelName": "string",
  "vehicleBrandName": "string"
}
```

#### **CreateCustomerVehicleDto** (Request)
```json
{
  "chassisNumber": "string (required, max 50)",
  "licensePlate": "string (optional, max 20)",  // ⭐ TRƯỜNG MỚI
  "color": "string (optional, max 50)",
  "year": "int (optional)",
  "initialKM": "decimal (optional)",
  "customerID": "string (required)",
  "modelID": "string (required)"
}
```

#### **UpdateCustomerVehicleDto** (Request)
```json
{
  "chassisNumber": "string (optional, max 50)",
  "licensePlate": "string (optional, max 20)",  // ⭐ TRƯỜNG MỚI
  "color": "string (optional, max 50)",
  "year": "int (optional)",
  "initialKM": "decimal (optional)",
  "modelID": "string (optional)"
}
```

### 🔧 **Các tính năng đã triển khai**

✅ **Model**: Thêm trường `LicensePlate` vào `CustomerVehicle`
✅ **DTOs**: Cập nhật và tạo mới các DTO với trường biển số
✅ **Service Layer**: `ICustomerVehicleService` và `CustomerVehicleService`
✅ **Controller**: `CustomerVehiclesController` với đầy đủ endpoints
✅ **AutoMapper**: Mapping profiles cho các DTO
✅ **Database Migration**: Thêm cột `LicensePlate` vào bảng `CustomerVehicles`
✅ **Dependency Injection**: Đăng ký service trong `Program.cs`
✅ **Validation**: Kiểm tra trùng lặp biển số và số khung
✅ **Error Handling**: Xử lý lỗi và exception handling

### 🎯 **Các bước tiếp theo**

1. **Áp dụng migration**: `dotnet ef database update`
2. **Test API**: Sử dụng Swagger hoặc Postman để test các endpoints
3. **Tích hợp Frontend**: Sử dụng các API endpoints trong ứng dụng frontend

### 📝 **Lưu ý quan trọng**

- Biển số xe có thể để trống (nullable) vì không phải xe nào cũng có biển số ngay
- Hệ thống sẽ kiểm tra trùng lặp biển số khi tạo mới hoặc cập nhật
- API hỗ trợ tìm kiếm xe theo biển số một cách nhanh chóng
- Tất cả endpoints đều có error handling và logging đầy đủ
