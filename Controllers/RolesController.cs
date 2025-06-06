using Microsoft.AspNetCore.Mvc; // Quan trọng để tạo API Controller
using Microsoft.EntityFrameworkCore; // Để dùng các phương thức của EF Core như .ToListAsync(), .FindAsync(), v.v.
using DecalXeAPI.Data; // Để truy cập ApplicationDbContext
using DecalXeAPI.Models; // Để truy cập các Models (Role)

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")] // Định nghĩa Route (đường dẫn) cho Controller này.
                               // [controller] sẽ tự động thay bằng tên Controller (trừ hậu tố "Controller"), tức là "Roles".
                               // Vậy API sẽ có dạng: /api/Roles
    [ApiController] // Thuộc tính này đánh dấu lớp là một API Controller, cung cấp các hành vi mặc định cho API (ví dụ: tự động trả về lỗi 400 nếu model không hợp lệ)
    public class RolesController : ControllerBase // Kế thừa từ ControllerBase (dành cho API không có View)
    {
        private readonly ApplicationDbContext _context; // Khai báo một biến để lưu trữ DbContext

        // Constructor: Hàm khởi tạo của Controller.
        // ASP.NET Core sẽ tự động "tiêm" (inject) ApplicationDbContext vào đây thông qua Dependency Injection.
        public RolesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API: GET api/Roles
        // Lấy tất cả các Role có trong database
        [HttpGet] // Thuộc tính [HttpGet] đánh dấu đây là một HTTP GET request
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
        {
            // Lấy tất cả Role từ bảng Roles trong database
            // .ToListAsync() là một phương thức của EF Core, chuyển đổi kết quả query thành danh sách bất đồng bộ.
            return await _context.Roles.ToListAsync();
        }

        // API: GET api/Roles/{id}
        // Lấy thông tin một Role theo RoleID
        [HttpGet("{id}")] // [HttpGet("{id}")] nghĩa là đường dẫn sẽ có thêm 1 tham số {id}.
                          // Ví dụ: /api/Roles/ADMIN
        public async Task<ActionResult<Role>> GetRole(string id)
        {
            // Tìm Role theo RoleID. .FindAsync() tìm kiếm theo khóa chính.
            var role = await _context.Roles.FindAsync(id);

            if (role == null) // Nếu không tìm thấy Role nào có ID này
            {
                return NotFound(); // Trả về lỗi 404 Not Found
            }

            return role; // Trả về Role tìm được
        }

        // API: POST api/Roles
        // Tạo một Role mới
        // [FromBody] Role role nghĩa là dữ liệu của Role sẽ được gửi trong phần body của HTTP request (dưới dạng JSON)
        [HttpPost]
        public async Task<ActionResult<Role>> PostRole(Role role)
        {
            // Thêm Role mới vào DbSet Roles (chưa lưu vào DB thật)
            _context.Roles.Add(role);
            // Lưu các thay đổi vào database
            await _context.SaveChangesAsync();

            // Trả về kết quả 201 Created (nghĩa là đã tạo thành công),
            // kèm theo thông tin của Role vừa tạo và đường dẫn để lấy Role đó.
            return CreatedAtAction(nameof(GetRole), new { id = role.RoleID }, role);
        }

        // API: PUT api/Roles/{id}
        // Cập nhật thông tin một Role hiện có
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRole(string id, Role role)
        {
            // Kiểm tra xem ID trong đường dẫn có khớp với RoleID trong body request không
            if (id != role.RoleID)
            {
                return BadRequest(); // Trả về lỗi 400 Bad Request nếu không khớp
            }

            // Đánh dấu Entity là đã được Modified (thay đổi)
            _context.Entry(role).State = EntityState.Modified;

            try
            {
                // Lưu các thay đổi vào database
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) // Xử lý lỗi nếu có xung đột cập nhật (ví dụ: Role không tồn tại)
            {
                if (!RoleExists(id)) // Kiểm tra xem Role có tồn tại không
                {
                    return NotFound(); // Nếu không tồn tại, trả về 404 Not Found
                }
                else
                {
                    throw; // Nếu là lỗi khác, ném lại lỗi
                }
            }

            return NoContent(); // Trả về 204 No Content (nghĩa là cập nhật thành công nhưng không có nội dung trả về)
        }

        // API: DELETE api/Roles/{id}
        // Xóa một Role
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            // Tìm Role cần xóa
            var role = await _context.Roles.FindAsync(id);
            if (role == null) // Nếu không tìm thấy
            {
                return NotFound(); // Trả về 404 Not Found
            }

            // Xóa Role khỏi DbSet Roles
            _context.Roles.Remove(role);
            // Lưu thay đổi vào database
            await _context.SaveChangesAsync();

            return NoContent(); // Trả về 204 No Content
        }

        // Hàm hỗ trợ: Kiểm tra xem Role có tồn tại không
        private bool RoleExists(string id)
        {
            return _context.Roles.Any(e => e.RoleID == id);
        }
    }
}