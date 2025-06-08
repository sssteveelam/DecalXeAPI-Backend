using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using AutoMapper;
using System.Collections.Generic;
using System;
using DecalXeAPI.QueryParams; // <-- THÊM DÒNG NÀY

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public OrdersController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // API: GET api/Orders
        // Lấy tất cả các Order, có hỗ trợ tìm kiếm, lọc, sắp xếp và phân trang
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders([FromQuery] OrderQueryParams queryParams) // <-- THÊM THAM SỐ queryParams
        {
            // Bắt đầu truy vấn
            var query = _context.Orders
                                .Include(o => o.Customer)
                                .Include(o => o.AssignedEmployee)
                                .Include(o => o.CustomServiceRequest)
                                .AsQueryable(); // Chuyển sang IQueryable để có thể xây dựng truy vấn động

            // 1. Lọc (Filtering)
            if (!string.IsNullOrEmpty(queryParams.Status))
            {
                query = query.Where(o => o.OrderStatus.ToLower() == queryParams.Status.ToLower());
            }

            // 2. Tìm kiếm (Searching)
            if (!string.IsNullOrEmpty(queryParams.SearchTerm))
            {
                query = query.Where(o =>
                    o.Customer.FirstName.ToLower().Contains(queryParams.SearchTerm.ToLower()) ||
                    o.Customer.LastName.ToLower().Contains(queryParams.SearchTerm.ToLower()) ||
                    o.AssignedEmployee.FirstName.ToLower().Contains(queryParams.SearchTerm.ToLower()) ||
                    o.AssignedEmployee.LastName.ToLower().Contains(queryParams.SearchTerm.ToLower()) ||
                    (o.CustomServiceRequest != null && o.CustomServiceRequest.Description.ToLower().Contains(queryParams.SearchTerm.ToLower()))
                );
            }

            // 3. Sắp xếp (Sorting)
            if (!string.IsNullOrEmpty(queryParams.SortBy))
            {
                switch (queryParams.SortBy.ToLower())
                {
                    case "orderdate":
                        query = queryParams.SortOrder.ToLower() == "desc" ? query.OrderByDescending(o => o.OrderDate) : query.OrderBy(o => o.OrderDate);
                        break;
                    case "totalamount":
                        query = queryParams.SortOrder.ToLower() == "desc" ? query.OrderByDescending(o => o.TotalAmount) : query.OrderBy(o => o.TotalAmount);
                        break;
                    case "customername":
                        query = queryParams.SortOrder.ToLower() == "desc" ? query.OrderByDescending(o => o.Customer.LastName).ThenByDescending(o => o.Customer.FirstName) : query.OrderBy(o => o.Customer.LastName).ThenBy(o => o.Customer.FirstName);
                        break;
                    case "orderstatus":
                        query = queryParams.SortOrder.ToLower() == "desc" ? query.OrderByDescending(o => o.OrderStatus) : query.OrderBy(o => o.OrderStatus);
                        break;
                    default: // Mặc định sắp xếp theo OrderDate nếu không hợp lệ
                        query = query.OrderBy(o => o.OrderDate);
                        break;
                }
            }
            else
            {
                // Luôn có một thứ tự sắp xếp mặc định để đảm bảo phân trang hoạt động đúng
                query = query.OrderBy(o => o.OrderDate);
            }


            // 4. Phân trang (Paging)
            var totalCount = await query.CountAsync(); // Lấy tổng số bản ghi trước khi phân trang
            var orders = await query
                                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize) // Bỏ qua các bản ghi ở trang trước
                                .Take(queryParams.PageSize) // Lấy số bản ghi cho trang hiện tại
                                .ToListAsync();

            // Ánh xạ sang DTO
            var orderDtos = _mapper.Map<List<OrderDto>>(orders);

            // Thêm thông tin phân trang vào Header của Response (Optional nhưng rất nên làm)
            Response.Headers.Add("X-Total-Count", totalCount.ToString());
            Response.Headers.Add("X-Page-Number", queryParams.PageNumber.ToString());
            Response.Headers.Add("X-Page-Size", queryParams.PageSize.ToString());
            Response.Headers.Add("X-Total-Pages", ((int)Math.Ceiling((double)totalCount / queryParams.PageSize)).ToString());


            return Ok(orderDtos);
        }

        // Các API GET by ID, POST, PUT, DELETE giữ nguyên
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(string id)
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

            var orderDto = _mapper.Map<OrderDto>(order);
            return Ok(orderDto);
        }

        [HttpPost]
        public async Task<ActionResult<OrderDto>> PostOrder(Order order)
        {
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
            if (!string.IsNullOrEmpty(order.CustomServiceRequest?.CustomRequestID))
            {
                var existingCsr = await _context.CustomServiceRequests
                                            .Where(csr => csr.CustomRequestID == order.CustomServiceRequest.CustomRequestID && csr.OrderID != null)
                                            .FirstOrDefaultAsync();
                if (existingCsr != null)
                {
                    return BadRequest("CustomServiceRequest này đã được liên kết với một Order khác.");
                }
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(order.CustomServiceRequest?.CustomRequestID))
            {
                var csr = await _context.CustomServiceRequests.FindAsync(order.CustomServiceRequest.CustomRequestID);
                if (csr != null)
                {
                    csr.OrderID = order.OrderID;
                    _context.CustomServiceRequests.Update(csr);
                    await _context.SaveChangesAsync();
                }
            }

            await _context.Entry(order).Reference(o => o.Customer).LoadAsync();
            await _context.Entry(order).Reference(o => o.AssignedEmployee).LoadAsync();
            await _context.Entry(order).Reference(o => o.CustomServiceRequest).LoadAsync();

            var orderDto = _mapper.Map<OrderDto>(order);
            return CreatedAtAction(nameof(GetOrder), new { id = orderDto.OrderID }, orderDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(string id, Order order)
        {
            if (id != order.OrderID)
            {
                return BadRequest();
            }

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


            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(order.CustomServiceRequest?.CustomRequestID))
                {
                    var csr = await _context.CustomServiceRequests.FindAsync(order.CustomServiceRequest.CustomRequestID);
                    if (csr != null && csr.OrderID != order.OrderID)
                    {
                        csr.OrderID = order.OrderID;
                        _context.CustomServiceRequests.Update(csr);
                        await _context.SaveChangesAsync();
                    }
                }

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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(string id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            var csr = await _context.CustomServiceRequests.FirstOrDefaultAsync(c => c.OrderID == id);
            if (csr != null)
            {
                csr.OrderID = null;
                _context.CustomServiceRequests.Update(csr);
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OrderExists(string id)
        {
            return _context.Orders.Any(e => e.OrderID == id);
        }

        private bool CustomerExists(string id)
        {
            return _context.Customers.Any(e => e.CustomerID == id);
        }

        private bool EmployeeExists(string id)
        {
            return _context.Employees.Any(e => e.EmployeeID == id);
        }

        private bool CustomServiceRequestExists(string id)
        {
            return _context.CustomServiceRequests.Any(e => e.CustomRequestID == id);
        }
    }
}