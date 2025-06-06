 using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DesignsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DesignsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API: GET api/Designs
        // Lấy tất cả các Design, bao gồm thông tin Order và Designer (Employee) liên quan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Design>>> GetDesigns()
        {
            return await _context.Designs
                                .Include(d => d.Order)
                                .Include(d => d.Designer)
                                .ToListAsync();
        }

        // API: GET api/Designs/{id}
        // Lấy thông tin một Design theo DesignID, bao gồm các thông tin liên quan
        [HttpGet("{id}")]
        public async Task<ActionResult<Design>> GetDesign(string id)
        {
            var design = await _context.Designs
                                        .Include(d => d.Order)
                                        .Include(d => d.Designer)
                                        .FirstOrDefaultAsync(d => d.DesignID == id);

            if (design == null)
            {
                return NotFound();
            }

            return design;
        }

        // API: POST api/Designs
        // Tạo một Design mới
        [HttpPost]
        public async Task<ActionResult<Design>> PostDesign(Design design)
        {
            // Kiểm tra xem OrderID và DesignerID có tồn tại không
            if (!string.IsNullOrEmpty(design.OrderID) && !OrderExists(design.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(design.DesignerID) && !EmployeeExists(design.DesignerID))
            {
                return BadRequest("DesignerID không tồn tại.");
            }

            _context.Designs.Add(design);
            await _context.SaveChangesAsync();

            // Tải lại thông tin liên quan để trả về đầy đủ
            await _context.Entry(design).Reference(d => d.Order).LoadAsync();
            await _context.Entry(design).Reference(d => d.Designer).LoadAsync();

            return CreatedAtAction(nameof(GetDesign), new { id = design.DesignID }, design);
        }

        // API: PUT api/Designs/{id}
        // Cập nhật thông tin một Design hiện có
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDesign(string id, Design design)
        {
            if (id != design.DesignID)
            {
                return BadRequest();
            }

            // Kiểm tra FKs trước khi cập nhật
            if (!string.IsNullOrEmpty(design.OrderID) && !OrderExists(design.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(design.DesignerID) && !EmployeeExists(design.DesignerID))
            {
                return BadRequest("DesignerID không tồn tại.");
            }

            _context.Entry(design).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DesignExists(id))
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

        // API: DELETE api/Designs/{id}
        // Xóa một Design
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDesign(string id)
        {
            var design = await _context.Designs.FindAsync(id);
            if (design == null)
            {
                return NotFound();
            }

            _context.Designs.Remove(design);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Hàm hỗ trợ: Kiểm tra xem Design có tồn tại không
        private bool DesignExists(string id)
        {
            return _context.Designs.Any(e => e.DesignID == id);
        }

        // Hàm hỗ trợ: Kiểm tra xem Order có tồn tại không (copy từ OrdersController, sẽ tạo sau)
        private bool OrderExists(string id)
        {
            return _context.Orders.Any(e => e.OrderID == id);
        }

        // Hàm hỗ trợ: Kiểm tra xem Employee có tồn tại không (copy từ EmployeesController)
        private bool EmployeeExists(string id)
        {
            return _context.Employees.Any(e => e.EmployeeID == id);
        }
    }
}