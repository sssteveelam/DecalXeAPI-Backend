using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using System; // Để dùng DateTime

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API: GET api/Orders
        // Lấy tất cả các Order, bao gồm thông tin Customer, AssignedEmployee và CustomServiceRequest liên quan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders
                                .Include(o => o.Customer)
                                .Include(o => o.AssignedEmployee)
                                .Include(o => o.CustomServiceRequest) // Kèm theo thông tin yêu cầu tùy chỉnh nếu có
                                .ToListAsync();
        }

        // API: GET api/Orders/{id}
        // Lấy thông tin một Order theo OrderID, bao gồm các thông tin liên quan
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(string id)
        {
            var order = await _context.Orders
                                    .Include(o => o.Customer)
                                    .Include(o => o.AssignedEmployee)
                                    .Include(o => o.CustomServiceRequest)
                                    .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // API: POST api/Orders
        // Tạo một Order mới
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            // Kiểm tra xem CustomerID và AssignedEmployeeID (nếu có) có tồn tại không
            if (!string.IsNullOrEmpty(order.CustomerID) && !CustomerExists(order.CustomerID))
            {
                return BadRequest("CustomerID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(order.AssignedEmployeeID) && !EmployeeExists(order.AssignedEmployeeID))
            {
                return BadRequest("AssignedEmployeeID không tồn tại.");
            }
            // Kiểm tra xem CustomRequestID có tồn tại không nếu được cung cấp
            // và đảm bảo nó chưa được gán cho Order nào khác (nếu là mối quan hệ 1-1)
            if (!string.IsNullOrEmpty(order.CustomServiceRequest?.CustomRequestID) && !CustomServiceRequestExists(order.CustomServiceRequest.CustomRequestID))
            {
                return BadRequest("CustomServiceRequestID không tồn tại.");
            }
             if (!string.IsNullOrEmpty(order.CustomServiceRequest?.CustomRequestID))
            {
                // Kiểm tra xem CustomServiceRequest này đã có Order liên kết chưa
                var existingCsr = await _context.CustomServiceRequests
                                            .Where(csr => csr.CustomRequestID == order.CustomServiceRequest.CustomRequestID && csr.OrderID != null)
                                            .FirstOrDefaultAsync();
                if (existingCsr != null)
                {
                    return BadRequest("CustomServiceRequest này đã được liên kết với một Order khác.");
                }
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // Lưu Order trước để có OrderID

            // Cập nhật OrderID cho CustomServiceRequest nếu có
            if (!string.IsNullOrEmpty(order.CustomServiceRequest?.CustomRequestID))
            {
                var csr = await _context.CustomServiceRequests.FindAsync(order.CustomServiceRequest.CustomRequestID);
                if (csr != null)
                {
                    csr.OrderID = order.OrderID; // Gán OrderID cho CustomServiceRequest
                    _context.CustomServiceRequests.Update(csr);
                    await _context.SaveChangesAsync();
                }
            }

            // Tải lại thông tin liên quan để trả về đầy đủ
            await _context.Entry(order).Reference(o => o.Customer).LoadAsync();
            await _context.Entry(order).Reference(o => o.AssignedEmployee).LoadAsync();
            await _context.Entry(order).Reference(o => o.CustomServiceRequest).LoadAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.OrderID }, order);
        }


        // API: PUT api/Orders/{id}
        // Cập nhật thông tin một Order hiện có
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(string id, Order order)
        {
            if (id != order.OrderID)
            {
                return BadRequest();
            }

            // Kiểm tra FKs trước khi cập nhật
            if (!string.IsNullOrEmpty(order.CustomerID) && !CustomerExists(order.CustomerID))
            {
                return BadRequest("CustomerID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(order.AssignedEmployeeID) && !EmployeeExists(order.AssignedEmployeeID))
            {
                return BadRequest("AssignedEmployeeID không tồn tại.");
            }
             if (!string.IsNullOrEmpty(order.CustomServiceRequest?.CustomRequestID) && !CustomServiceRequestExists(order.CustomServiceRequest.CustomRequestID))
            {
                return BadRequest("CustomServiceRequestID không tồn tại.");
            }
            // Logic kiểm tra CSR đã được liên kết hay chưa, cần phức tạp hơn cho PUT
            // Tạm thời bỏ qua kiểm tra trùng lặp CSR cho PUT để tránh phức tạp ban đầu
            // Nếu cần, có thể kiểm tra xem CSR đó có đang được gán cho Order khác không (trừ chính Order này)


            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                // Cập nhật OrderID cho CustomServiceRequest nếu có sự thay đổi
                // Điều này cần được xử lý cẩn thận nếu muốn gán lại CSR cho Order khác
                // hoặc bỏ gán. Tạm thời, logic đơn giản là nếu CSR được cung cấp, gán OrderID.
                if (!string.IsNullOrEmpty(order.CustomServiceRequest?.CustomRequestID))
                {
                    var csr = await _context.CustomServiceRequests.FindAsync(order.CustomServiceRequest.CustomRequestID);
                    if (csr != null && csr.OrderID != order.OrderID) // Nếu CSR này chưa gán hoặc gán sai
                    {
                        csr.OrderID = order.OrderID;
                        _context.CustomServiceRequests.Update(csr);
                        await _context.SaveChangesAsync();
                    }
                }
                // Xử lý trường hợp nếu trước đó có CSR, giờ muốn hủy liên kết
                // (Cần logic phức tạp hơn, có thể dùng DTO để nhận biết sự thay đổi)

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // API: DELETE api/Orders/{id}
        // Xóa một Order
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(string id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // Nếu Order này có liên kết với CustomServiceRequest, cần ngắt liên kết trước khi xóa Order
            var csr = await _context.CustomServiceRequests.FirstOrDefaultAsync(c => c.OrderID == id);
            if (csr != null)
            {
                csr.OrderID = null; // Ngắt liên kết
                _context.CustomServiceRequests.Update(csr);
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Hàm hỗ trợ: Kiểm tra xem Order có tồn tại không
        private bool OrderExists(string id)
        {
            return _context.Orders.Any(e => e.OrderID == id);
        }

        // Hàm hỗ trợ: Kiểm tra xem Customer có tồn tại không (copy từ CustomersController)
        private bool CustomerExists(string id)
        {
            return _context.Customers.Any(e => e.CustomerID == id);
        }

        // Hàm hỗ trợ: Kiểm tra xem Employee có tồn tại không (copy từ EmployeesController)
        private bool EmployeeExists(string id)
        {
            return _context.Employees.Any(e => e.EmployeeID == id);
        }

        // Hàm hỗ trợ: Kiểm tra xem CustomServiceRequest có tồn tại không
        private bool CustomServiceRequestExists(string id)
        {
            return _context.CustomServiceRequests.Any(e => e.CustomRequestID == id);
        }
    }
}