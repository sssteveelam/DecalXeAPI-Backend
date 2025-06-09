using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs; // Thêm dòng này
using AutoMapper; // Thêm dòng này
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")]
    public class StoresController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper; // Khai báo

        public StoresController(ApplicationDbContext context, IMapper mapper) // Tiêm IMapper
        {
            _context = context;
            _mapper = mapper;
        }

        // API: GET api/Stores
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StoreDto>>> GetStores() // Kiểu trả về DTO
        {
            var stores = await _context.Stores.ToListAsync();
            return Ok(_mapper.Map<List<StoreDto>>(stores)); // Dùng AutoMapper
        }

        // API: GET api/Stores/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<StoreDto>> GetStore(string id) // Kiểu trả về DTO
        {
            var store = await _context.Stores.FindAsync(id);

            if (store == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<StoreDto>(store)); // Dùng AutoMapper
        }

        // API: POST api/Stores
        [HttpPost]
        public async Task<ActionResult<StoreDto>> PostStore(Store store) // Kiểu trả về DTO
        {
            _context.Stores.Add(store);
            await _context.SaveChangesAsync();

            // Không cần LoadAsync() vì StoreDto không có Navigation Property cần tải
            // Nếu có, cần load ở đây

            return CreatedAtAction(nameof(GetStore), new { id = store.StoreID }, _mapper.Map<StoreDto>(store)); // Dùng AutoMapper
        }

        // PUT và DELETE không thay đổi kiểu trả về là IActionResult
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStore(string id, Store store)
        {
            if (id != store.StoreID)
            {
                return BadRequest();
            }
            _context.Entry(store).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StoreExists(id)) { return NotFound(); } else { throw; }
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStore(string id)
        {
            var store = await _context.Stores.FindAsync(id);
            if (store == null) { return NotFound(); }
            _context.Stores.Remove(store);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        private bool StoreExists(string id) { return _context.Stores.Any(e => e.StoreID == id); }
    }
}