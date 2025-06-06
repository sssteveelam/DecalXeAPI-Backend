using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using System;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarrantiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WarrantiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API: GET api/Warranties
        // Lấy tất cả các Warranty, bao gồm thông tin Order liên quan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Warranty>>> GetWarranties()
        {
            return await _context.Warranties.Include(w => w.Order).ToListAsync();
        }

        // API: GET api/Warranties/{id}
        // Lấy thông tin một Warranty theo WarrantyID, bao gồm Order liên quan
        [HttpGet("{id}")]
        public async Task<ActionResult<Warranty>> GetWarranty(string id)
        {
            var warranty = await _context.Warranties.Include(w => w.Order).FirstOrDefaultAsync(w => w.WarrantyID == id);

            if (warranty == null)
            {
                return NotFound();
            }

            return warranty;
        }

        // API: POST api/Warranties
        // Tạo một Warranty mới
        [HttpPost]
        public async Task<ActionResult<Warranty>> PostWarranty(Warranty warranty)
        {
            // Kiểm tra OrderID có tồn tại không
            if (!string.IsNullOrEmpty(warranty.OrderID) && !OrderExists(warranty.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }

            _context.Warranties.Add(warranty);
            await _context.SaveChangesAsync();

            // Tải lại thông tin Order để trả về đầy đủ
            await _context.Entry(warranty).Reference(w => w.Order).LoadAsync();

            return CreatedAtAction(nameof(GetWarranty), new { id = warranty.WarrantyID }, warranty);
        }

        // API: PUT api/Warranties/{id}
        // Cập nhật thông tin một Warranty hiện có
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWarranty(string id, Warranty warranty)
        {
            if (id != warranty.WarrantyID)
            {
                return BadRequest();
            }

            // Kiểm tra OrderID trước khi cập nhật
            if (!string.IsNullOrEmpty(warranty.OrderID) && !OrderExists(warranty.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }

            _context.Entry(warranty).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WarrantyExists(id))
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

        // API: DELETE api/Warranties/{id}
        // Xóa một Warranty
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWarranty(string id)
        {
            var warranty = await _context.Warranties.FindAsync(id);
            if (warranty == null)
            {
                return NotFound();
            }

            _context.Warranties.Remove(warranty);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Hàm hỗ trợ: Kiểm tra xem Warranty có tồn tại không
        private bool WarrantyExists(string id)
        {
            return _context.Warranties.Any(e => e.WarrantyID == id);
        }

        // Hàm hỗ trợ: Kiểm tra xem Order có tồn tại không (copy từ OrdersController)
        private bool OrderExists(string id)
        {
            return _context.Orders.Any(e => e.OrderID == id);
        }
    }
}