using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs; // Để sử dụng CustomServiceRequestDto
using AutoMapper; // Để sử dụng AutoMapper
using System.Collections.Generic; // Để sử dụng IEnumerable
using System; // Để sử dụng DateTime

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomServiceRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper; // Khai báo biến IMapper

        public CustomServiceRequestsController(ApplicationDbContext context, IMapper mapper) // Tiêm IMapper
        {
            _context = context;
            _mapper = mapper;
        }

        // API: GET api/CustomServiceRequests
        // Lấy tất cả các CustomServiceRequest, bao gồm thông tin Customer, SalesEmployee và Order liên quan, trả về DTO
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomServiceRequestDto>>> GetCustomServiceRequests() // Kiểu trả về là CustomServiceRequestDto
        {
            var customServiceRequests = await _context.CustomServiceRequests
                                                    .Include(csr => csr.Customer)
                                                    .Include(csr => csr.SalesEmployee)
                                                    .Include(csr => csr.Order) // Mối quan hệ 1-1 với Order
                                                    .ToListAsync();
            // Sử dụng AutoMapper để ánh xạ từ List<CustomServiceRequest> sang List<CustomServiceRequestDto>
            var customServiceRequestDtos = _mapper.Map<List<CustomServiceRequestDto>>(customServiceRequests);
            return Ok(customServiceRequestDtos);
        }

        // API: GET api/CustomServiceRequests/{id}
        // Lấy thông tin một CustomServiceRequest theo CustomRequestID, bao gồm các thông tin liên quan, trả về DTO
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomServiceRequestDto>> GetCustomServiceRequest(string id) // Kiểu trả về là CustomServiceRequestDto
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

            // Sử dụng AutoMapper để ánh xạ từ CustomServiceRequest Model sang CustomServiceRequestDto
            var customServiceRequestDto = _mapper.Map<CustomServiceRequestDto>(customServiceRequest);
            return Ok(customServiceRequestDto);
        }

        // API: POST api/CustomServiceRequests
        // Tạo một CustomServiceRequest mới, nhận vào CustomServiceRequest Model, trả về CustomServiceRequestDto sau khi tạo
        [HttpPost]
        public async Task<ActionResult<CustomServiceRequestDto>> PostCustomServiceRequest(CustomServiceRequest customServiceRequest) // Kiểu trả về là CustomServiceRequestDto
        {
            // Kiểm tra FKs có tồn tại không
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

            _context.CustomServiceRequests.Add(customServiceRequest);
            await _context.SaveChangesAsync();

            // Tải lại thông tin liên quan để AutoMapper có thể ánh xạ đầy đủ
            await _context.Entry(customServiceRequest).Reference(csr => csr.Customer).LoadAsync();
            await _context.Entry(customServiceRequest).Reference(csr => csr.SalesEmployee).LoadAsync();
            await _context.Entry(customServiceRequest).Reference(csr => csr.Order).LoadAsync();

            // Ánh xạ CustomServiceRequest Model vừa tạo sang CustomServiceRequestDto để trả về
            var customServiceRequestDto = _mapper.Map<CustomServiceRequestDto>(customServiceRequest);
            return CreatedAtAction(nameof(GetCustomServiceRequest), new { id = customServiceRequestDto.CustomRequestID }, customServiceRequestDto);
        }

        // API: PUT api/CustomServiceRequests/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomServiceRequest(string id, CustomServiceRequest customServiceRequest)
        {
            if (id != customServiceRequest.CustomRequestID)
            {
                return BadRequest();
            }

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

        private bool CustomServiceRequestExists(string id)
        {
            return _context.CustomServiceRequests.Any(e => e.CustomRequestID == id);
        }

        private bool CustomerExists(string id)
        {
            return _context.Customers.Any(e => e.CustomerID == id);
        }

        private bool EmployeeExists(string id)
        {
            return _context.Employees.Any(e => e.EmployeeID == id);
        }

        private bool OrderExists(string id)
        {
            return _context.Orders.Any(e => e.OrderID == id);
        }
    }
}