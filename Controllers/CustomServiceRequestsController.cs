using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs; 
using AutoMapper;
using System.Collections.Generic; 
using System; 
using System.Linq;
using Microsoft.AspNetCore.Authorization; 

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class CustomServiceRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper; // Khai báo biến IMapper

        // Tiêm ApplicationDbContext và IMapper vào constructor
        public CustomServiceRequestsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // API: GET api/CustomServiceRequests
        // Lấy tất cả các CustomServiceRequest, bao gồm các thông tin liên quan, trả về DTO
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Sales,Designer,Customer")] // <-- Chỉ định Role có thể xem (nếu muốn hạn chế hơn)
        public async Task<ActionResult<IEnumerable<CustomServiceRequestDto>>> GetCustomServiceRequests()
        {
            // Bao gồm các Navigation Property cần thiết để AutoMapper có đủ dữ liệu ánh xạ
            var customServiceRequests = await _context.CustomServiceRequests
                                                        .Include(csr => csr.Customer)
                                                        .Include(csr => csr.SalesEmployee)
                                                        .Include(csr => csr.Order)
                                                        .ToListAsync();
            // Ánh xạ danh sách CustomServiceRequest Models sang CustomServiceRequest DTOs
            var customServiceRequestDtos = _mapper.Map<List<CustomServiceRequestDto>>(customServiceRequests);
            return Ok(customServiceRequestDtos);
        }

        // API: GET api/CustomServiceRequests/{id}
        // Lấy thông tin một CustomServiceRequest theo ID, bao gồm các thông tin liên quan, trả về DTO
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Sales,Designer,Customer")] // <-- Chỉ định Role có thể xem (nếu muốn hạn chế hơn)
        public async Task<ActionResult<CustomServiceRequestDto>> GetCustomServiceRequest(string id)
        {
            // Bao gồm các Navigation Property cần thiết để AutoMapper có đủ dữ liệu ánh xạ
            var customServiceRequest = await _context.CustomServiceRequests
                                                        .Include(csr => csr.Customer)
                                                        .Include(csr => csr.SalesEmployee)
                                                        .Include(csr => csr.Order)
                                                        .FirstOrDefaultAsync(csr => csr.CustomRequestID == id);

            if (customServiceRequest == null)
            {
                return NotFound(); // Trả về 404 nếu không tìm thấy
            }

            // Ánh xạ CustomServiceRequest Model sang CustomServiceRequest DTO
            var customServiceRequestDto = _mapper.Map<CustomServiceRequestDto>(customServiceRequest);
            return Ok(customServiceRequestDto);
        }

        // API: POST api/CustomServiceRequests
        // API này sẽ khởi tạo một yêu cầu dịch vụ tùy chỉnh mới từ CreateCustomServiceRequestDto
        [HttpPost]
        [Authorize(Roles = "Customer,Sales")] // <-- Chỉ Role Customer và Sales được phép POST

        public async Task<ActionResult<CustomServiceRequestDto>> PostCustomServiceRequest(
            [FromBody] CreateCustomServiceRequestDto createDto) // Nhận vào DTO đầu vào
        {
            // --- LOGIC NGHIỆP VỤ: VALIDATE INPUT DTO VÀ KIỂM TRA FKs ---
            // CustomerID là bắt buộc và phải tồn tại
            if (!string.IsNullOrEmpty(createDto.CustomerID) && !CustomerExists(createDto.CustomerID))
            {
                return BadRequest("CustomerID không tồn tại.");
            }
            // SalesEmployeeID (nếu có) phải tồn tại
            if (!string.IsNullOrEmpty(createDto.SalesEmployeeID) && !EmployeeExists(createDto.SalesEmployeeID))
            {
                return BadRequest("SalesEmployeeID không tồn tại.");
            }

            // --- LOGIC NGHIỆP VỤ: ÁNH XẠ DTO SANG ENTITY VÀ THÊM VÀO DB ---
            // AutoMapper sẽ tự động gán CustomRequestID (Guid.NewGuid()), RequestDate (DateTime.UtcNow),
            // và RequestStatus ("New") dựa trên cấu hình trong MappingProfile.
            var customServiceRequest = _mapper.Map<CustomServiceRequest>(createDto);

            _context.CustomServiceRequests.Add(customServiceRequest);
            await _context.SaveChangesAsync(); // Lưu CustomServiceRequest mới

            // --- LOGIC NGHIỆP VỤ: TẢI LẠI THÔNG TIN LIÊN QUAN VÀ TRẢ VỀ DTO KẾT QUẢ ---
            // Cần tải lại các Navigation Property để AutoMapper có đủ dữ liệu cho DTO đầu ra
            await _context.Entry(customServiceRequest).Reference(csr => csr.Customer).LoadAsync();
            await _context.Entry(customServiceRequest).Reference(csr => csr.SalesEmployee).LoadAsync();
            // Order sẽ là null ở bước này vì nó chưa được liên kết với một Order

            // Ánh xạ CustomServiceRequest Model vừa tạo sang CustomServiceRequest DTO
            var customServiceRequestDto = _mapper.Map<CustomServiceRequestDto>(customServiceRequest);

            // Trả về 201 Created và DTO của yêu cầu vừa tạo
            return CreatedAtAction(nameof(GetCustomServiceRequest), new { id = customServiceRequestDto.CustomRequestID }, customServiceRequestDto);
        }

        // API: PUT api/CustomServiceRequests/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager,Sales,Designer")] // <-- Chỉ Admin, Manager, Sales, Designer được phép PUT
        public async Task<IActionResult> PutCustomServiceRequest(string id, CustomServiceRequest customServiceRequest)
        {
            if (id != customServiceRequest.CustomRequestID)
            {
                return BadRequest();
            }

            // Kiểm tra FKs trước khi cập nhật
            if (!string.IsNullOrEmpty(customServiceRequest.CustomerID) && !CustomerExists(customServiceRequest.CustomerID))
            {
                return BadRequest("CustomerID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(customServiceRequest.SalesEmployeeID) && !EmployeeExists(customServiceRequest.SalesEmployeeID))
            {
                return BadRequest("SalesEmployeeID không tồn tại.");
            }
            // OrderID có thể null, chỉ kiểm tra nếu có giá trị và tồn tại
            if (!string.IsNullOrEmpty(customServiceRequest.OrderID) && !OrderExists(customServiceRequest.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }

            _context.Entry(customServiceRequest).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomServiceRequestExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // Cập nhật thành công, không trả về nội dung
        }

        // API: DELETE api/CustomServiceRequests/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")] // <-- Chỉ Admin, Manager được phép DELETE
        public async Task<IActionResult> DeleteCustomServiceRequest(string id)
        {
            var customServiceRequest = await _context.CustomServiceRequests.FindAsync(id);
            if (customServiceRequest == null)
            {
                return NotFound();
            }

            // Nếu yêu cầu này đã được liên kết với một Order, cần ngắt liên kết (nếu Order vẫn tồn tại)
            if (!string.IsNullOrEmpty(customServiceRequest.OrderID))
            {
                var order = await _context.Orders.FindAsync(customServiceRequest.OrderID);
                if (order != null && order.CustomServiceRequest != null && order.CustomServiceRequest.CustomRequestID == customServiceRequest.CustomRequestID)
                {
                    // Đây là một logic phức tạp hơn, tùy thuộc vào nghiệp vụ
                    // Ví dụ: có thể đặt trạng thái của Order thành "Cancelled"
                    // Hoặc xóa Order luôn nếu nó chỉ được tạo từ CSR này
                    // Để đơn giản, hiện tại chỉ xóa CSR. Nếu muốn Order tự hủy/chuyển trạng thái, cần thêm logic ở đây
                }
            }

            _context.CustomServiceRequests.Remove(customServiceRequest);
            await _context.SaveChangesAsync();

            return NoContent(); // Xóa thành công, không trả về nội dung
        }

        // API: POST api/CustomServiceRequests/{id}/convertToOrder
        // Chuyển đổi một CustomServiceRequest thành một Order
        [HttpPost("{id}/convertToOrder")]
        [Authorize(Roles = "Admin,Manager,Sales")] // <-- Chỉ Admin, Manager, Sales được phép chuyển đổi
        public async Task<ActionResult<OrderDto>> ConvertToOrder(string id, [FromBody] ConvertCsrToOrderDto convertDto)
        {
            // --- BƯỚC 1: LẤY CUSTOMSERVICEREQUEST VÀ KIỂM TRA TÍNH HỢP LỆ ---
            var customServiceRequest = await _context.CustomServiceRequests.FindAsync(id);
            if (customServiceRequest == null)
            {
                return NotFound("Yêu cầu dịch vụ tùy chỉnh không tồn tại.");
            }

            // Kiểm tra trạng thái yêu cầu (chỉ cho phép chuyển đổi khi New hoặc PendingEstimate)
            if (customServiceRequest.RequestStatus != "New" && customServiceRequest.RequestStatus != "PendingEstimate")
            {
                return BadRequest($"Yêu cầu dịch vụ tùy chỉnh đang ở trạng thái '{customServiceRequest.RequestStatus}' và không thể chuyển đổi thành đơn hàng.");
            }

            // Kiểm tra xem yêu cầu đã có Order liên kết chưa
            if (!string.IsNullOrEmpty(customServiceRequest.OrderID))
            {
                return BadRequest("Yêu cầu dịch vụ tùy chỉnh này đã được liên kết với một đơn hàng khác.");
            }

            // Đảm bảo EstimatedCost và EstimatedWorkUnits đã có
            if (!customServiceRequest.EstimatedCost.HasValue || !customServiceRequest.EstimatedWorkUnits.HasValue)
            {
                return BadRequest("Yêu cầu dịch vụ tùy chỉnh chưa có ước tính chi phí và số lượng công việc.");
            }

            // Kiểm tra AssignedEmployeeID và CustomServiceServiceID có tồn tại không
            if (!EmployeeExists(convertDto.AssignedEmployeeID))
            {
                return BadRequest("AssignedEmployeeID không tồn tại.");
            }
            if (!DecalServiceExists(convertDto.CustomServiceServiceID))
            {
                return BadRequest("CustomServiceServiceID không tồn tại. Vui lòng đảm bảo dịch vụ tùy chỉnh chung đã được tạo.");
            }

            // --- BƯỚC 2: TẠO ORDER MỚI TỪ DỮ LIỆU ĐẦU VÀO VÀ CUSTOMSERVICEREQUEST ---
            var newOrder = _mapper.Map<Order>(convertDto); // Ánh xạ từ DTO đầu vào
            newOrder.CustomerID = customServiceRequest.CustomerID; // Gán CustomerID từ CustomServiceRequest
            // newOrder.TotalAmount và OrderStatus đã được AutoMapper gán từ EstimatedCost và "New"

            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync(); // Lưu Order trước để có OrderID

            // --- BƯỚC 3: TẠO ORDERDETAIL CHO DỊCH VỤ TÙY CHỈNH ---
            var customOrderDetail = new OrderDetail
            {
                OrderDetailID = Guid.NewGuid().ToString(),
                OrderID = newOrder.OrderID,
                ServiceID = convertDto.CustomServiceServiceID, // Sử dụng ServiceID của dịch vụ tùy chỉnh chung
                Quantity = 1, // Dịch vụ tùy chỉnh thường là 1 lần
                Price = newOrder.TotalAmount // Lấy giá từ TotalAmount của Order (EstimatedCost)
            };
            _context.OrderDetails.Add(customOrderDetail);
            await _context.SaveChangesAsync();

            // --- BƯỚC 4: CẬP NHẬT TRẠNG THÁI CỦA CUSTOMSERVICEREQUEST ---
            customServiceRequest.OrderID = newOrder.OrderID; // Liên kết CustomServiceRequest với Order
            customServiceRequest.RequestStatus = "ConvertedToOrder"; // Cập nhật trạng thái
            customServiceRequest.EstimatedCost = convertDto.EstimatedCost; // Cập nhật lại EstimatedCost (nếu có thay đổi)
            customServiceRequest.EstimatedWorkUnits = convertDto.EstimatedWorkUnits; // Cập nhật lại EstimatedWorkUnits (nếu có thay đổi)

            _context.CustomServiceRequests.Update(customServiceRequest);
            await _context.SaveChangesAsync();

            // --- BƯỚC 5: TẢI LẠI THÔNG TIN VÀ TRẢ VỀ ORDER DTO ---
            // Tải lại các Navigation Property của Order mới để trả về DTO đầy đủ
            await _context.Entry(newOrder).Reference(o => o.Customer).LoadAsync();
            await _context.Entry(newOrder).Reference(o => o.AssignedEmployee).LoadAsync();
            await _context.Entry(newOrder).Reference(o => o.CustomServiceRequest).LoadAsync();

            var newOrderDto = _mapper.Map<OrderDto>(newOrder);
            return CreatedAtAction("GetOrder", "Orders", new { id = newOrderDto.OrderID }, newOrderDto);
        }

        // --- HÀM HỖ TRỢ: KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG ---
        private bool CustomerExists(string id) { return _context.Customers.Any(e => e.CustomerID == id); }
        private bool EmployeeExists(string id) { return _context.Employees.Any(e => e.EmployeeID == id); }
        private bool OrderExists(string id) { return _context.Orders.Any(e => e.OrderID == id); }
        private bool CustomServiceRequestExists(string id) { return _context.CustomServiceRequests.Any(e => e.CustomRequestID == id); }
        private bool DecalServiceExists(string id) { return _context.DecalServices.Any(e => e.ServiceID == id); }
    }
}