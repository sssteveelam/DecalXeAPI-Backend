using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DecalServicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DecalServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API: GET api/DecalServices
        // Lấy tất cả các DecalService, bao gồm thông tin DecalType liên quan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DecalService>>> GetDecalServices()
        {
            // .Include(ds => ds.DecalType) sẽ tải thông tin DecalType liên kết.
            return await _context.DecalServices.Include(ds => ds.DecalType).ToListAsync();
        }

        // API: GET api/DecalServices/{id}
        // Lấy thông tin một DecalService theo ServiceID, bao gồm DecalType liên quan
        [HttpGet("{id}")]
        public async Task<ActionResult<DecalService>> GetDecalService(string id)
        {
            var decalService = await _context.DecalServices.Include(ds => ds.DecalType).FirstOrDefaultAsync(ds => ds.ServiceID == id);

            if (decalService == null)
            {
                return NotFound();
            }

            return decalService;
        }

        // API: POST api/DecalServices
        // Tạo một DecalService mới
        [HttpPost]
        public async Task<ActionResult<DecalService>> PostDecalService(DecalService decalService)
        {
            // Kiểm tra xem DecalTypeID có tồn tại không
            if (!string.IsNullOrEmpty(decalService.DecalTypeID) && !DecalTypeExists(decalService.DecalTypeID))
            {
                return BadRequest("DecalTypeID không tồn tại.");
            }

            _context.DecalServices.Add(decalService);
            await _context.SaveChangesAsync();

            // Tải lại thông tin DecalType để trả về đầy đủ cho client
            await _context.Entry(decalService).Reference(ds => ds.DecalType).LoadAsync();

            return CreatedAtAction(nameof(GetDecalService), new { id = decalService.ServiceID }, decalService);
        }

        // API: PUT api/DecalServices/{id}
        // Cập nhật thông tin một DecalService hiện có
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDecalService(string id, DecalService decalService)
        {
            if (id != decalService.ServiceID)
            {
                return BadRequest();
            }

            // Kiểm tra xem DecalTypeID có tồn tại không trước khi cập nhật
            if (!string.IsNullOrEmpty(decalService.DecalTypeID) && !DecalTypeExists(decalService.DecalTypeID))
            {
                return BadRequest("DecalTypeID không tồn tại.");
            }

            _context.Entry(decalService).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DecalServiceExists(id))
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

        // API: DELETE api/DecalServices/{id}
        // Xóa một DecalService
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDecalService(string id)
        {
            var decalService = await _context.DecalServices.FindAsync(id);
            if (decalService == null)
            {
                return NotFound();
            }

            _context.DecalServices.Remove(decalService);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Hàm hỗ trợ: Kiểm tra xem DecalService có tồn tại không
        private bool DecalServiceExists(string id)
        {
            return _context.DecalServices.Any(e => e.ServiceID == id);
        }

        // Hàm hỗ trợ: Kiểm tra xem DecalType có tồn tại không (copy từ DecalTypesController)
        private bool DecalTypeExists(string id)
        {
            return _context.DecalTypes.Any(e => e.DecalTypeID == id);
        }
    }
}