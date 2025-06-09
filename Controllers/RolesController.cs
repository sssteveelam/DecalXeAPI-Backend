using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs; // Để sử dụng RoleDto (nếu có, hoặc sử dụng Model trực tiếp nếu DTO không thêm gì mới)
using AutoMapper; // Để sử dụng AutoMapper
using System.Collections.Generic; // Để sử dụng IEnumerable
using Microsoft.AspNetCore.Authorization;


namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // <-- THÊM DÒNG NÀY: Chỉ Admin mới được quản lý Roles
    public class RolesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper; // Khai báo biến IMapper

        public RolesController(ApplicationDbContext context, IMapper mapper) // Tiêm IMapper
        {
            _context = context;
            _mapper = mapper;
        }

        // API: GET api/Roles
        // Lấy tất cả các Role, trả về dưới dạng RoleDto (hoặc Role Model nếu RoleDto không có sự khác biệt)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles() // Kiểu trả về là RoleDto
        {
            var roles = await _context.Roles.ToListAsync();
            // Sử dụng AutoMapper để ánh xạ từ List<Role> sang List<RoleDto>
            var roleDtos = _mapper.Map<List<RoleDto>>(roles);
            return Ok(roleDtos);
        }

        // API: GET api/Roles/{id}
        // Lấy thông tin một Role theo RoleID, trả về dưới dạng RoleDto
        [HttpGet("{id}")]
        public async Task<ActionResult<RoleDto>> GetRole(string id) // Kiểu trả về là RoleDto
        {
            var role = await _context.Roles.FindAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            // Sử dụng AutoMapper để ánh xạ từ Role Model sang RoleDto
            var roleDto = _mapper.Map<RoleDto>(role);
            return Ok(roleDto);
        }

        // API: POST api/Roles
        // Tạo một Role mới, nhận vào Role Model, trả về RoleDto sau khi tạo
        [HttpPost]
        public async Task<ActionResult<RoleDto>> PostRole(Role role) // Kiểu trả về là RoleDto
        {
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            // Không cần LoadAsync() vì RoleDto không có Navigation Property cần tải

            // Ánh xạ Role Model vừa tạo sang RoleDto để trả về
            var roleDto = _mapper.Map<RoleDto>(role);
            return CreatedAtAction(nameof(GetRole), new { id = roleDto.RoleID }, roleDto);
        }

        // API: PUT api/Roles/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRole(string id, Role role)
        {
            if (id != role.RoleID)
            {
                return BadRequest();
            }

            _context.Entry(role).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoleExists(id))
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

        // API: DELETE api/Roles/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RoleExists(string id)
        {
            return _context.Roles.Any(e => e.RoleID == id);
        }
    }
}