using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs; // Để sử dụng OrderDetailDto
using AutoMapper; // Để sử dụng AutoMapper
using System.Collections.Generic; // Để sử dụng IEnumerable

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper; // Khai báo biến IMapper

        public OrderDetailsController(ApplicationDbContext context, IMapper mapper) // Tiêm IMapper
        {
            _context = context;
            _mapper = mapper;
        }

        // API: GET api/OrderDetails
        // Lấy tất cả các OrderDetail, bao gồm thông tin Order và DecalService liên quan, trả về DTO
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDetailDto>>> GetOrderDetails() // Kiểu trả về là OrderDetailDto
        {
            var orderDetails = await _context.OrderDetails
                                            .Include(od => od.Order)
                                            .Include(od => od.DecalService)
                                            .ToListAsync();
            // Sử dụng AutoMapper để ánh xạ từ List<OrderDetail> sang List<OrderDetailDto>
            var orderDetailDtos = _mapper.Map<List<OrderDetailDto>>(orderDetails);
            return Ok(orderDetailDtos);
        }

        // API: GET api/OrderDetails/{id}
        // Lấy thông tin một OrderDetail theo OrderDetailID, bao gồm các thông tin liên quan, trả về DTO
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDetailDto>> GetOrderDetail(string id) // Kiểu trả về là OrderDetailDto
        {
            var orderDetail = await _context.OrderDetails
                                                .Include(od => od.Order)
                                                .Include(od => od.DecalService)
                                                .FirstOrDefaultAsync(od => od.OrderDetailID == id);

            if (orderDetail == null)
            {
                return NotFound();
            }

            // Sử dụng AutoMapper để ánh xạ từ OrderDetail Model sang OrderDetailDto
            var orderDetailDto = _mapper.Map<OrderDetailDto>(orderDetail);
            return Ok(orderDetailDto);
        }

        // API: POST api/OrderDetails
        // Tạo một OrderDetail mới, nhận vào OrderDetail Model, trả về OrderDetailDto sau khi tạo
        [HttpPost]
        public async Task<ActionResult<OrderDetailDto>> PostOrderDetail(OrderDetail orderDetail) // Kiểu trả về là OrderDetailDto
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

            // Tải lại thông tin liên quan để AutoMapper có thể ánh xạ đầy đủ
            await _context.Entry(orderDetail).Reference(od => od.Order).LoadAsync();
            await _context.Entry(orderDetail).Reference(od => od.DecalService).LoadAsync();

            // Ánh xạ OrderDetail Model vừa tạo sang OrderDetailDto để trả về
            var orderDetailDto = _mapper.Map<OrderDetailDto>(orderDetail);
            return CreatedAtAction(nameof(GetOrderDetail), new { id = orderDetailDto.OrderDetailID }, orderDetailDto);
        }

        // API: PUT api/OrderDetails/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrderDetail(string id, OrderDetail orderDetail)
        {
            if (id != orderDetail.OrderDetailID)
            {
                return BadRequest();
            }

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

        private bool OrderDetailExists(string id)
        {
            return _context.OrderDetails.Any(e => e.OrderDetailID == id);
        }

        private bool OrderExists(string id)
        {
            return _context.Orders.Any(e => e.OrderID == id);
        }

        private bool DecalServiceExists(string id)
        {
            return _context.DecalServices.Any(e => e.ServiceID == id);
        }
    }
}