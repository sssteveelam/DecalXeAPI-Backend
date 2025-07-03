using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data; // Vẫn cần DbContext để dùng các hàm Exists đơn giản
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces; // <-- THÊM DÒNG NÀY (Để sử dụng ICustomServiceRequestService)
using AutoMapper;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace DecalXeAPI.Controllers
{
    


    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomServiceRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Vẫn giữ để dùng các hàm Exists đơn giản
        private readonly ICustomServiceRequestService _customServiceRequestService; // <-- KHAI BÁO BIẾN CHO SERVICE
        private readonly IMapper _mapper; // Vẫn giữ để ánh xạ DTOs nếu có
        private readonly ILogger<CustomServiceRequestsController> _logger;

        public CustomServiceRequestsController(ApplicationDbContext context, ICustomServiceRequestService customServiceRequestService, IMapper mapper, ILogger<CustomServiceRequestsController> logger) // <-- TIÊM ICustomServiceRequestService
        {
            _context = context; // Để dùng các hàm hỗ trợ
            _customServiceRequestService = customServiceRequestService; // Gán Service
            _mapper = mapper;
            _logger = logger;
        }

        // API: GET api/CustomServiceRequests
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Sales,Designer,Customer")]
        public async Task<ActionResult<IEnumerable<CustomServiceRequestDto>>> GetCustomServiceRequests()
        {
            // Ủy quyền logic cho Service Layer
            var customServiceRequests = await _customServiceRequestService.GetCustomServiceRequestsAsync();
            return Ok(customServiceRequests);
        }

        // API: GET api/CustomServiceRequests/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Sales,Designer,Customer")]
        public async Task<ActionResult<CustomServiceRequestDto>> GetCustomServiceRequest(string id)
        {
            // Ủy quyền logic cho Service Layer
            var customServiceRequestDto = await _customServiceRequestService.GetCustomServiceRequestByIdAsync(id);

            if (customServiceRequestDto == null)
            {
                return NotFound();
            }

            return Ok(customServiceRequestDto);
        }

        // API: POST api/CustomServiceRequests
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Customer,Sales")] 
        public async Task<ActionResult<CustomServiceRequestDto>> PostCustomServiceRequest(
            [FromBody] CreateCustomServiceRequestDto createDto)
        {
            // Ủy quyền logic tạo yêu cầu cho Service Layer
            var (createdDto, errorMessage) = await _customServiceRequestService.CreateCustomServiceRequestAsync(createDto);

            if (createdDto == null)
            {
                return BadRequest(errorMessage); // Trả về lỗi từ Service
            }

            return CreatedAtAction(nameof(GetCustomServiceRequest), new { id = createdDto.CustomRequestID }, createdDto);
        }

        // API: PUT api/CustomServiceRequests/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager,Sales,Designer")]
        public async Task<IActionResult> PutCustomServiceRequest(string id, CustomServiceRequest customServiceRequest)
        {
            // Ủy quyền logic cập nhật cho Service Layer
            var success = await _customServiceRequestService.UpdateCustomServiceRequestAsync(id, customServiceRequest);

            if (!success)
            {
                return NotFound(); // Service trả về false nếu không tìm thấy
            }

            return NoContent();
        }

        // API: DELETE api/CustomServiceRequests/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteCustomServiceRequest(string id)
        {
            // Ủy quyền logic xóa cho Service Layer
            var success = await _customServiceRequestService.DeleteCustomServiceRequestAsync(id);

            if (!success)
            {
                return NotFound(); // Service trả về false nếu không tìm thấy
            }

            return NoContent();
        }

        // API: POST api/CustomServiceRequests/{id}/convertToOrder
        [HttpPost("{id}/convertToOrder")]
        [Authorize(Roles = "Admin,Manager,Sales")]
        public async Task<ActionResult<OrderDto>> ConvertToOrder(string id, [FromBody] ConvertCsrToOrderDto convertDto)
        {
            // Ủy quyền logic chuyển đổi cho Service Layer
            var (orderDto, errorMessage) = await _customServiceRequestService.ConvertToOrderAsync(id, convertDto);

            if (orderDto == null)
            {
                return BadRequest(errorMessage); // Trả về lỗi từ Service
            }

            return CreatedAtAction("GetOrder", "Orders", new { id = orderDto.OrderID }, orderDto);
        }

        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (Vẫn giữ ở Controller để kiểm tra FKs) ---
        private bool CustomerExists(string id) { return _context.Customers.Any(e => e.CustomerID == id); }
        private bool EmployeeExists(string id) { return _context.Employees.Any(e => e.EmployeeID == id); }
        private bool OrderExists(string id) { return _context.Orders.Any(e => e.OrderID == id); }
        private bool CustomServiceRequestExists(string id) { return _context.CustomServiceRequests.Any(e => e.CustomRequestID == id); }
        private bool DecalServiceExists(string id) { return _context.DecalServices.Any(e => e.ServiceID == id); } // Cần để kiểm tra FK cho ConvertToOrder
    }
}