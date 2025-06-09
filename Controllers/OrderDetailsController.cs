using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization; // <-- THÊM DÒNG NÀY

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // <-- MẶC ĐỊNH TẤT CẢ API TRONG CONTROLLER NÀY ĐỀU CẦN XÁC THỰC
    public class OrderDetailsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public OrderDetailsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // API: GET api/OrderDetails
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Sales,Technician")] // <-- NỚI LỎNG QUYỀN CHO API GET
        public async Task<ActionResult<IEnumerable<OrderDetailDto>>> GetOrderDetails()
        {
            var orderDetails = await _context.OrderDetails
                                            .Include(od => od.Order)
                                            .Include(od => od.DecalService)
                                            .ToListAsync();
            var orderDetailDtos = _mapper.Map<List<OrderDetailDto>>(orderDetails);
            return Ok(orderDetailDtos);
        }

        // API: GET api/OrderDetails/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Sales,Technician")] // <-- NỚI LỎNG QUYỀN CHO API GET BY ID
        public async Task<ActionResult<OrderDetailDto>> GetOrderDetail(string id)
        {
            var orderDetail = await _context.OrderDetails
                                                .Include(od => od.Order)
                                                .Include(od => od.DecalService)
                                                .FirstOrDefaultAsync(od => od.OrderDetailID == id);

            if (orderDetail == null)
            {
                return NotFound();
            }

            var orderDetailDto = _mapper.Map<OrderDetailDto>(orderDetail);
            return Ok(orderDetailDto);
        }

        // API: POST api/OrderDetails
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Sales")] // <-- Chỉ Admin, Manager, Sales được phép POST
        public async Task<ActionResult<OrderDetailDto>> PostOrderDetail(OrderDetail orderDetail)
        {
            if (!string.IsNullOrEmpty(orderDetail.OrderID) && !OrderExists(orderDetail.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(orderDetail.ServiceID) && !DecalServiceExists(orderDetail.ServiceID))
            {
                return BadRequest("ServiceID không tồn tại.");
            }

            var service = await _context.DecalServices
                                        .Include(s => s.ServiceProducts)
                                            .ThenInclude(sp => sp.Product)
                                        .FirstOrDefaultAsync(s => s.ServiceID == orderDetail.ServiceID);

            if (service == null)
            {
                return BadRequest("Dịch vụ không tồn tại hoặc không tìm thấy thông tin sản phẩm liên quan.");
            }

            orderDetail.Price = service.Price;

            foreach (var sp in service.ServiceProducts)
            {
                if (sp.Product != null)
                {
                    var quantityToDeduct = sp.QuantityUsed * orderDetail.Quantity;
                    if (sp.Product.StockQuantity < quantityToDeduct)
                    {
                        return BadRequest($"Sản phẩm '{sp.Product.ProductName}' không đủ tồn kho. Chỉ còn {sp.Product.StockQuantity} {sp.Product.Unit} trong kho, nhưng cần {quantityToDeduct} {sp.Product.Unit}.");
                    }
                    sp.Product.StockQuantity -= (int)quantityToDeduct;
                    _context.Products.Update(sp.Product);
                }
            }
            await _context.SaveChangesAsync();

            _context.OrderDetails.Add(orderDetail);
            await _context.SaveChangesAsync();

            await RecalculateOrderTotalAmount(orderDetail.OrderID);

            await _context.Entry(orderDetail).Reference(od => od.Order).LoadAsync();
            await _context.Entry(orderDetail).Reference(od => od.DecalService).LoadAsync();

            var orderDetailDto = _mapper.Map<OrderDetailDto>(orderDetail);
            return CreatedAtAction(nameof(GetOrderDetail), new { id = orderDetailDto.OrderDetailID }, orderDetailDto);
        }

        // API: PUT api/OrderDetails/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager,Sales")] // <-- Chỉ Admin, Manager, Sales được phép PUT
        public async Task<IActionResult> PutOrderDetail(string id, OrderDetail orderDetail)
        {
            if (id != orderDetail.OrderDetailID)
            {
                return BadRequest();
            }

            var oldOrderDetail = await _context.OrderDetails
                                                .Include(od => od.DecalService)
                                                    .ThenInclude(ds => ds.ServiceProducts)
                                                        .ThenInclude(sp => sp.Product)
                                                .FirstOrDefaultAsync(od => od.OrderDetailID == id);
            if (oldOrderDetail == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(orderDetail.OrderID) && !OrderExists(orderDetail.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(orderDetail.ServiceID) && !DecalServiceExists(orderDetail.ServiceID))
            {
                return BadRequest("ServiceID không tồn tại.");
            }

            var newService = await _context.DecalServices
                                            .Include(s => s.ServiceProducts)
                                                .ThenInclude(sp => sp.Product)
                                            .FirstOrDefaultAsync(s => s.ServiceID == orderDetail.ServiceID);
            if (newService == null)
            {
                return BadRequest("Dịch vụ mới không tồn tại.");
            }

            orderDetail.Price = newService.Price;

            if (oldOrderDetail.Quantity != orderDetail.Quantity || oldOrderDetail.ServiceID != orderDetail.ServiceID)
            {
                foreach (var sp in oldOrderDetail.DecalService.ServiceProducts)
                {
                    if (sp.Product != null)
                    {
                        sp.Product.StockQuantity += (int)(sp.QuantityUsed * oldOrderDetail.Quantity);
                        _context.Products.Update(sp.Product);
                    }
                }

                foreach (var sp in newService.ServiceProducts)
                {
                    if (sp.Product != null)
                    {
                        var quantityToDeduct = sp.QuantityUsed * orderDetail.Quantity;
                        if (sp.Product.StockQuantity < quantityToDeduct)
                        {
                            foreach (var rollbackSp in oldOrderDetail.DecalService.ServiceProducts)
                            {
                                if (rollbackSp.Product != null)
                                {
                                    rollbackSp.Product.StockQuantity -= (int)(rollbackSp.QuantityUsed * oldOrderDetail.Quantity);
                                    _context.Products.Update(rollbackSp.Product);
                                }
                            }
                            await _context.SaveChangesAsync();
                            return BadRequest($"Sản phẩm '{sp.Product.ProductName}' không đủ tồn kho cho yêu cầu mới. Chỉ còn {sp.Product.StockQuantity} {sp.Product.Unit}.");
                        }
                        sp.Product.StockQuantity -= (int)quantityToDeduct;
                        _context.Products.Update(sp.Product);
                    }
                }
            }
            await _context.SaveChangesAsync();

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

            await RecalculateOrderTotalAmount(orderDetail.OrderID);

            return NoContent();
        }

        // API: DELETE api/OrderDetails/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager,Sales")] // <-- Chỉ Admin, Manager, Sales được phép DELETE
        public async Task<IActionResult> DeleteOrderDetail(string id)
        {
            var orderDetail = await _context.OrderDetails
                                            .Include(od => od.DecalService)
                                                .ThenInclude(ds => ds.ServiceProducts)
                                                    .ThenInclude(sp => sp.Product)
                                            .FirstOrDefaultAsync(od => od.OrderDetailID == id);
            if (orderDetail == null)
            {
                return NotFound();
            }

            _context.OrderDetails.Remove(orderDetail);
            await _context.SaveChangesAsync();

            if (orderDetail.DecalService != null && orderDetail.DecalService.ServiceProducts != null)
            {
                foreach (var sp in orderDetail.DecalService.ServiceProducts)
                {
                    if (sp.Product != null)
                    {
                        sp.Product.StockQuantity += (int)(sp.QuantityUsed * orderDetail.Quantity);
                        _context.Products.Update(sp.Product);
                    }
                }
                await _context.SaveChangesAsync();
            }

            await RecalculateOrderTotalAmount(orderDetail.OrderID);

            return NoContent();
        }

        // Hàm hỗ trợ: Tính toán lại tổng tiền của một Order
        private async Task RecalculateOrderTotalAmount(string orderId)
        {
            var order = await _context.Orders
                                    .Include(o => o.OrderDetails)
                                    .FirstOrDefaultAsync(o => o.OrderID == orderId);

            if (order != null)
            {
                order.TotalAmount = order.OrderDetails.Sum(od => od.Price * od.Quantity);
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
            }
        }

        // --- HÀM HỖ TRỢ: KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG ---
        private bool OrderDetailExists(string id) { return _context.OrderDetails.Any(e => e.OrderDetailID == id); }
        private bool OrderExists(string id) { return _context.Orders.Any(e => e.OrderID == id); }
        private bool DecalServiceExists(string id) { return _context.DecalServices.Any(e => e.ServiceID == id); }
    }
}