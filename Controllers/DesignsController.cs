using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization; // <-- THÊM DÒNG NÀY

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,Designer")] // <-- MẶC ĐỊNH CHO CONTROLLER: Admin, Manager, Designer
    public class DesignsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public DesignsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // API: GET api/Designs
        // Cho phép thêm Sales xem các bản thiết kế
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Designer,Sales")] // <-- NỚI LỎNG QUYỀN CHO API GET: Bao gồm Sales
        public async Task<ActionResult<IEnumerable<DesignDto>>> GetDesigns()
        {
            var designs = await _context.Designs
                                    .Include(d => d.Order)
                                    .Include(d => d.Designer)
                                    .ToListAsync();
            var designDtos = _mapper.Map<List<DesignDto>>(designs);
            return Ok(designDtos);
        }

        // API: GET api/Designs/{id}
        // Cho phép thêm Sales xem chi tiết bản thiết kế
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Designer,Sales")] // <-- NỚI LỎNG QUYỀN CHO API GET BY ID
        public async Task<ActionResult<DesignDto>> GetDesign(string id)
        {
            var design = await _context.Designs
                                        .Include(d => d.Order)
                                        .Include(d => d.Designer)
                                        .FirstOrDefaultAsync(d => d.DesignID == id);

            if (design == null)
            {
                return NotFound();
            }

            var designDto = _mapper.Map<DesignDto>(design);
            return Ok(designDto);
        }

        // API: POST api/Designs
        // Giữ quyền như Controller Level: Admin, Manager, Designer
        [HttpPost]
        public async Task<ActionResult<DesignDto>> PostDesign(Design design)
        {
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

            await _context.Entry(design).Reference(d => d.Order).LoadAsync();
            await _context.Entry(design).Reference(d => d.Designer).LoadAsync();

            var designDto = _mapper.Map<DesignDto>(design);
            return CreatedAtAction(nameof(GetDesign), new { id = designDto.DesignID }, designDto);
        }

        // API: PUT api/Designs/{id}
        // Giữ quyền như Controller Level: Admin, Manager, Designer
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDesign(string id, Design design)
        {
            if (id != design.DesignID)
            {
                return BadRequest();
            }

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
        // Giữ quyền như Controller Level: Admin, Manager, Designer
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

        private bool DesignExists(string id) { return _context.Designs.Any(e => e.DesignID == id); }
        private bool OrderExists(string id) { return _context.Orders.Any(e => e.OrderID == id); }
        private bool EmployeeExists(string id) { return _context.Employees.Any(e => e.EmployeeID == id); }
    }
}