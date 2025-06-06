using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrderDetailsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API: GET api/OrderDetails
        // Lấy tất cả các OrderDetail, bao gồm thông tin Order và DecalService liên quan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDetail>>> GetOrderDetails()
        {
            return await _context.OrderDetails
                                .Include(od => od.Order)
                                .Include(od => od.DecalService)
                                .ToListAsync();
        }

        // API: GET api/OrderDetails/{id}
        // Lấy thông tin một OrderDetail theo OrderDetailID, bao gồm các thông tin liên quan
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDetail>> GetOrderDetail(string id)
        {
            var orderDetail = await _context.OrderDetails
                                            .Include(od => od.Order)
                                            .Include(od => od.DecalService)
                                            .FirstOrDefaultAsync(od => od.OrderDetailID == id);

            if (orderDetail == null)
            {
                return NotFound();
            }

            return orderDetail;
        }

        // API: POST api/OrderDetails
        // Tạo một OrderDetail mới
        [HttpPost]
        public async Task<ActionResult<OrderDetail>> PostOrderDetail(OrderDetail orderDetail)
        {
            // Kiểm tra xem OrderID và ServiceID có tồn tại không
            if (!string.IsNullOrEmpty(orderDetail.OrderID) && !OrderExists(orderDetail.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(orderDetail.ServiceID) && !DecalServiceExists(orderDetail.ServiceID))
            {
                return BadRequest("ServiceID không tồn tại.");
            }

            _context.OrderDetails.Add(orderDetail);
            await _context.SaveChangesAsync();

            // Tải lại thông tin liên quan để trả về đầy đủ
            await _context.Entry(orderDetail).Reference(od => od.Order).LoadAsync();
            await _context.Entry(orderDetail).Reference(od => od.DecalService).LoadAsync();

            return CreatedAtAction(nameof(GetOrderDetail), new { id = orderDetail.OrderDetailID }, orderDetail);
        }

        // API: PUT api/OrderDetails/{id}
        // Cập nhật thông tin một OrderDetail hiện có
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrderDetail(string id, OrderDetail orderDetail)
        {
            if (id != orderDetail.OrderDetailID)
            {
                return BadRequest();
            }

            // Kiểm tra FKs trước khi cập nhật
            if (!string.IsNullOrEmpty(orderDetail.OrderID) && !OrderExists(orderDetail.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(orderDetail.ServiceID) && !DecalServiceExists(orderDetail.ServiceID))
            {
                return BadRequest("ServiceID không tồn tại.");
            }

            _context.Entry(orderDetail).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderDetailExists(id))
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

        // API: DELETE api/OrderDetails/{id}
        // Xóa một OrderDetail
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderDetail(string id)
        {
            var orderDetail = await _context.OrderDetails.FindAsync(id);
            if (orderDetail == null)
            {
                return NotFound();
            }

            _context.OrderDetails.Remove(orderDetail);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Hàm hỗ trợ: Kiểm tra xem OrderDetail có tồn tại không
        private bool OrderDetailExists(string id)
        {
            return _context.OrderDetails.Any(e => e.OrderDetailID == id);
        }

        // Hàm hỗ trợ: Kiểm tra xem Order có tồn tại không (copy từ OrdersController)
        private bool OrderExists(string id)
        {
            return _context.Orders.Any(e => e.OrderID == id);
        }

        // Hàm hỗ trợ: Kiểm tra xem DecalService có tồn tại không (copy từ DecalServicesController)
        private bool DecalServiceExists(string id)
        {
            return _context.DecalServices.Any(e => e.ServiceID == id);
        }
    }
}