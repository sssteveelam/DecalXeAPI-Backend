using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomServiceRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CustomServiceRequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API: GET api/CustomServiceRequests
        // Lấy tất cả các CustomServiceRequest, bao gồm thông tin Customer và SalesEmployee liên quan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomServiceRequest>>> GetCustomServiceRequests()
        {
            return await _context.CustomServiceRequests
                                .Include(csr => csr.Customer)
                                .Include(csr => csr.SalesEmployee)
                                .Include(csr => csr.Order) // Mối quan hệ 1-1 với Order
                                .ToListAsync();
        }

        // API: GET api/CustomServiceRequests/{id}
        // Lấy thông tin một CustomServiceRequest theo CustomRequestID, bao gồm các thông tin liên quan
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomServiceRequest>> GetCustomServiceRequest(string id)
        {
            var customServiceRequest = await _context.CustomServiceRequests
                                                        .Include(csr => csr.Customer)
                                                        .Include(csr => csr.SalesEmployee)
                                                        .Include(csr => csr.Order)
                                                        .FirstOrDefaultAsync(csr => csr.CustomRequestID == id);

            if (customServiceRequest == null)
            {
                return NotFound();
            }

            return customServiceRequest;
        }

        // API: POST api/CustomServiceRequests
        // Tạo một CustomServiceRequest mới
        [HttpPost]
        public async Task<ActionResult<CustomServiceRequest>> PostCustomServiceRequest(CustomServiceRequest customServiceRequest)
        {
            // Kiểm tra xem CustomerID có tồn tại không
            if (!string.IsNullOrEmpty(customServiceRequest.CustomerID) && !CustomerExists(customServiceRequest.CustomerID))
            {
                return BadRequest("CustomerID không tồn tại.");
            }
            // Kiểm tra xem SalesEmployeeID có tồn tại không nếu được cung cấp
            if (!string.IsNullOrEmpty(customServiceRequest.SalesEmployeeID) && !EmployeeExists(customServiceRequest.SalesEmployeeID))
            {
                return BadRequest("SalesEmployeeID không tồn tại.");
            }
            // Kiểm tra xem OrderID có tồn tại không nếu được cung cấp
            // Mặc dù OrderID trong CSR có thể là NULL, nhưng nếu có giá trị thì phải tồn tại
            if (!string.IsNullOrEmpty(customServiceRequest.OrderID) && !OrderExists(customServiceRequest.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }


            _context.CustomServiceRequests.Add(customServiceRequest);
            await _context.SaveChangesAsync();

            // Tải lại thông tin liên quan để trả về đầy đủ
            await _context.Entry(customServiceRequest).Reference(csr => csr.Customer).LoadAsync();
            await _context.Entry(customServiceRequest).Reference(csr => csr.SalesEmployee).LoadAsync();
            await _context.Entry(customServiceRequest).Reference(csr => csr.Order).LoadAsync();


            return CreatedAtAction(nameof(GetCustomServiceRequest), new { id = customServiceRequest.CustomRequestID }, customServiceRequest);
        }

        // API: PUT api/CustomServiceRequests/{id}
        // Cập nhật thông tin một CustomServiceRequest hiện có
        [HttpPut("{id}")]
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

            return NoContent();
        }

        // API: DELETE api/CustomServiceRequests/{id}
        // Xóa một CustomServiceRequest
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomServiceRequest(string id)
        {
            var customServiceRequest = await _context.CustomServiceRequests.FindAsync(id);
            if (customServiceRequest == null)
            {
                return NotFound();
            }

            _context.CustomServiceRequests.Remove(customServiceRequest);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Hàm hỗ trợ: Kiểm tra xem CustomServiceRequest có tồn tại không
        private bool CustomServiceRequestExists(string id)
        {
            return _context.CustomServiceRequests.Any(e => e.CustomRequestID == id);
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

        // Hàm hỗ trợ: Kiểm tra xem Order có tồn tại không
        private bool OrderExists(string id)
        {
            return _context.Orders.Any(e => e.OrderID == id);
        }
    }
}