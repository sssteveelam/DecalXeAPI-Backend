using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.Services.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace DecalXeAPI.Services.Implementations
{
    public class CustomServiceRequestService : ICustomServiceRequestService // <-- Kế thừa từ ICustomServiceRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomServiceRequestService> _logger;
        private readonly IOrderService _orderService; // Cần dùng OrderService để tạo Order
        private readonly IOrderDetailService _orderDetailService;

        public CustomServiceRequestService(ApplicationDbContext context, IMapper mapper, ILogger<CustomServiceRequestService> logger, IOrderService orderService, IOrderDetailService orderDetailService)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _orderService = orderService; // Tiêm IOrderService
            _orderDetailService = orderDetailService;
        }

        // Lấy danh sách yêu cầu tùy chỉnh
        public async Task<IEnumerable<CustomServiceRequestDto>> GetCustomServiceRequestsAsync()
        {
            _logger.LogInformation("Lấy danh sách yêu cầu dịch vụ tùy chỉnh.");
            var customServiceRequests = await _context.CustomServiceRequests
                                                        .Include(csr => csr.Customer)
                                                        .Include(csr => csr.SalesEmployee)
                                                        .Include(csr => csr.Order)
                                                        .ToListAsync();
            var customServiceRequestDtos = _mapper.Map<List<CustomServiceRequestDto>>(customServiceRequests);
            return customServiceRequestDtos;
        }

        // Lấy yêu cầu tùy chỉnh theo ID
        public async Task<CustomServiceRequestDto?> GetCustomServiceRequestByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy yêu cầu dịch vụ tùy chỉnh với ID: {CustomRequestID}", id);
            var customServiceRequest = await _context.CustomServiceRequests
                                                        .Include(csr => csr.Customer)
                                                        .Include(csr => csr.SalesEmployee)
                                                        .Include(csr => csr.Order)
                                                        .FirstOrDefaultAsync(csr => csr.CustomRequestID == id);

            if (customServiceRequest == null)
            {
                _logger.LogWarning("Không tìm thấy yêu cầu dịch vụ tùy chỉnh với ID: {CustomRequestID}", id);
                return null;
            }

            var customServiceRequestDto = _mapper.Map<CustomServiceRequestDto>(customServiceRequest);
            _logger.LogInformation("Đã trả về yêu cầu dịch vụ tùy chỉnh với ID: {CustomRequestID}", id);
            return customServiceRequestDto;
        }

        // Tạo yêu cầu tùy chỉnh mới
        public async Task<(CustomServiceRequestDto? CustomServiceRequest, string? ErrorMessage)> CreateCustomServiceRequestAsync(CreateCustomServiceRequestDto createDto)
        {
            _logger.LogInformation("Yêu cầu tạo yêu cầu dịch vụ tùy chỉnh mới cho CustomerID: {CustomerID}", createDto.CustomerID);

            if (!await CustomerExistsAsync(createDto.CustomerID))
            {
                return (null, "CustomerID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(createDto.SalesEmployeeID) && !await EmployeeExistsAsync(createDto.SalesEmployeeID))
            {
                return (null, "SalesEmployeeID không tồn tại.");
            }

            var customServiceRequest = _mapper.Map<CustomServiceRequest>(createDto);

            _context.CustomServiceRequests.Add(customServiceRequest);
            await _context.SaveChangesAsync();

            await _context.Entry(customServiceRequest).Reference(csr => csr.Customer).LoadAsync();
            await _context.Entry(customServiceRequest).Reference(csr => csr.SalesEmployee).LoadAsync();

            var customServiceRequestDto = _mapper.Map<CustomServiceRequestDto>(customServiceRequest);
            _logger.LogInformation("Đã tạo yêu cầu dịch vụ tùy chỉnh mới với ID: {CustomRequestID}", customServiceRequest.CustomRequestID);
            return (customServiceRequestDto, null);
        }

        // Cập nhật yêu cầu tùy chỉnh
        public async Task<bool> UpdateCustomServiceRequestAsync(string id, CustomServiceRequest customServiceRequest)
        {
            _logger.LogInformation("Yêu cầu cập nhật yêu cầu dịch vụ tùy chỉnh với ID: {CustomRequestID}", id);

            if (id != customServiceRequest.CustomRequestID)
            {
                _logger.LogWarning("ID trong tham số ({Id}) không khớp với CustomRequestID trong body ({CustomRequestIDBody})", id, customServiceRequest.CustomRequestID);
                return false;
            }

            if (!await CustomServiceRequestExistsAsync(id))
            {
                _logger.LogWarning("Không tìm thấy yêu cầu dịch vụ tùy chỉnh để cập nhật với ID: {CustomRequestID}", id);
                return false;
            }

            if (!await CustomerExistsAsync(customServiceRequest.CustomerID))
            {
                return false;
            }
            if (!string.IsNullOrEmpty(customServiceRequest.SalesEmployeeID) && !await EmployeeExistsAsync(customServiceRequest.SalesEmployeeID))
            {
                return false;
            }
            if (!string.IsNullOrEmpty(customServiceRequest.OrderID) && !await OrderExistsAsync(customServiceRequest.OrderID))
            {
                return false;
            }

            _context.Entry(customServiceRequest).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật yêu cầu dịch vụ tùy chỉnh với ID: {CustomRequestID}", id);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật yêu cầu dịch vụ tùy chỉnh với ID: {CustomRequestID}", id);
                throw;
            }
        }

        // Xóa yêu cầu tùy chỉnh
        public async Task<bool> DeleteCustomServiceRequestAsync(string id)
        {
            _logger.LogInformation("Yêu cầu xóa yêu cầu dịch vụ tùy chỉnh với ID: {CustomRequestID}", id);
            var customServiceRequest = await _context.CustomServiceRequests.FindAsync(id);
            if (customServiceRequest == null)
            {
                _logger.LogWarning("Không tìm thấy yêu cầu dịch vụ tùy chỉnh để xóa với ID: {CustomRequestID}", id);
                return false;
            }

            if (!string.IsNullOrEmpty(customServiceRequest.OrderID))
            {
                _logger.LogWarning("Không thể xóa yêu cầu dịch vụ tùy chỉnh {CustomRequestID} vì nó đã liên kết với Order {OrderID}. Vui lòng xử lý Order trước.", customServiceRequest.CustomRequestID, customServiceRequest.OrderID);
                return false; // Hoặc thêm logic để ngắt liên kết nếu nghiệp vụ cho phép
            }

            _context.CustomServiceRequests.Remove(customServiceRequest);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa yêu cầu dịch vụ tùy chỉnh với ID: {CustomRequestID}", id);
            return true;
        }

        // Chuyển đổi yêu cầu thành Order (logic nghiệp vụ chính)
        public async Task<(OrderDto? Order, string? ErrorMessage)> ConvertToOrderAsync(string id, ConvertCsrToOrderDto convertDto)
        {
            _logger.LogInformation("Yêu cầu chuyển đổi CustomServiceRequest {CustomRequestID} thành Order.", id);

            var customServiceRequest = await _context.CustomServiceRequests.FindAsync(id);
            if (customServiceRequest == null)
            {
                return (null, "Yêu cầu dịch vụ tùy chỉnh không tồn tại.");
            }

            if (customServiceRequest.RequestStatus != "New" && customServiceRequest.RequestStatus != "PendingEstimate")
            {
                return (null, $"Yêu cầu dịch vụ tùy chỉnh đang ở trạng thái '{customServiceRequest.RequestStatus}' và không thể chuyển đổi thành đơn hàng.");
            }

            if (!string.IsNullOrEmpty(customServiceRequest.OrderID))
            {
                return (null, "Yêu cầu dịch vụ tùy chỉnh này đã được liên kết với một đơn hàng khác.");
            }

            if (!customServiceRequest.EstimatedCost.HasValue || !customServiceRequest.EstimatedWorkUnits.HasValue)
            {
                return (null, "Yêu cầu dịch vụ tùy chỉnh chưa có ước tính chi phí và số lượng công việc.");
            }

            if (!await EmployeeExistsAsync(convertDto.AssignedEmployeeID))
            {
                return (null, "AssignedEmployeeID không tồn tại.");
            }
            if (!await DecalServiceExistsAsync(convertDto.CustomServiceServiceID))
            {
                return (null, "CustomServiceServiceID không tồn tại. Vui lòng đảm bảo dịch vụ tùy chỉnh chung đã được tạo.");
            }

            // Tạo Order mới thông qua OrderService
            // (Đây là ví dụ về việc một Service gọi một Service khác)
            var newOrderModel = _mapper.Map<Order>(convertDto);
            newOrderModel.CustomerID = customServiceRequest.CustomerID; // Gán CustomerID từ CSR
            // TotalAmount và OrderStatus được gán trong MappingProfile của ConvertCsrToOrderDto -> Order

            var newOrderDto = await _orderService.CreateOrderAsync(newOrderModel); // Gọi OrderService để tạo Order
            if (newOrderDto == null)
            {
                _logger.LogError("Không thể tạo Order từ CustomServiceRequest {CustomRequestID}.", id);
                return (null, "Không thể tạo đơn hàng từ yêu cầu tùy chỉnh.");
            }

            // Tạo OrderDetail cho dịch vụ tùy chỉnh
            var customOrderDetail = new OrderDetail
            {
                OrderDetailID = Guid.NewGuid().ToString(),
                OrderID = newOrderDto.OrderID, // Lấy OrderID từ Order vừa tạo
                ServiceID = convertDto.CustomServiceServiceID,
                Quantity = 1,
                Price = newOrderDto.TotalAmount // Lấy giá từ Order DTO (EstimatedCost)
            };
            // Lưu ý: Logic cập nhật tồn kho khi thêm OrderDetail sẽ nằm trong OrderDetailService
            // Gọi OrderDetailService để tạo OrderDetail
            var (createdOrderDetailDto, orderDetailError) = await _orderDetailService.CreateOrderDetailAsync(customOrderDetail);
            if (createdOrderDetailDto == null)
            {
                _logger.LogError("Không thể tạo OrderDetail cho Order {OrderID} từ CustomServiceRequest {CustomRequestID}. Lỗi: {Error}", newOrderDto.OrderID, id, orderDetailError);
                // Nếu không thể tạo OrderDetail, cần xóa Order vừa tạo để đảm bảo toàn vẹn dữ liệu
                await _orderService.DeleteOrderAsync(newOrderDto.OrderID);
                return (null, orderDetailError);
            }

            // Cập nhật trạng thái của CustomServiceRequest
            customServiceRequest.OrderID = newOrderDto.OrderID;
            customServiceRequest.RequestStatus = "ConvertedToOrder";
            customServiceRequest.EstimatedCost = convertDto.EstimatedCost;
            customServiceRequest.EstimatedWorkUnits = convertDto.EstimatedWorkUnits;

            _context.CustomServiceRequests.Update(customServiceRequest);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Đã chuyển đổi CustomServiceRequest {CustomRequestID} thành Order {OrderID}", id, newOrderDto.OrderID);
            return (newOrderDto, null);
        }

        // --- HÀM HỖ TRỢ: KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (CHUYỂN SANG PUBLIC) ---
        public async Task<bool> CustomerExistsAsync(string id) { return await _context.Customers.AnyAsync(e => e.CustomerID == id); }
        public async Task<bool> EmployeeExistsAsync(string id) { return await _context.Employees.AnyAsync(e => e.EmployeeID == id); }
        public async Task<bool> OrderExistsAsync(string id) { return await _context.Orders.AnyAsync(e => e.OrderID == id); }
        public async Task<bool> DecalServiceExistsAsync(string id) { return await _context.DecalServices.AnyAsync(e => e.ServiceID == id); }
        public async Task<bool> CustomServiceRequestExistsAsync(string id) // <-- ĐẢM BẢO CÁI NÀY LÀ PUBLIC
        {
            return await _context.CustomServiceRequests.AnyAsync(e => e.CustomRequestID == id);
        }
    }
}