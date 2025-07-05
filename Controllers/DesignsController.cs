// DecalXeAPI/Controllers/DesignsController.cs
using Microsoft.AspNetCore.Mvc;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;
using AutoMapper; // <--- THÊM DÒNG NÀY VÀO
using DecalXeAPI.Data; // <--- THÊM DÒNG NÀY ĐỂ BIẾT ApplicationDbContext

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,Designer")]
    public class DesignsController : ControllerBase
    {
        private readonly IDesignService _designService;
        private readonly IMapper _mapper; // Thêm IMapper
        private readonly ApplicationDbContext _context;

        public DesignsController(IDesignService designService, IMapper mapper, ApplicationDbContext context)
        {
            _designService = designService;
            _mapper = mapper;
            _context = context; 
        }

        // GET: api/Designs
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Designer,Sales")]
        public async Task<ActionResult<IEnumerable<DesignDto>>> GetDesigns()
        {
            return Ok(await _designService.GetDesignsAsync());
        }

        // GET: api/Designs/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Designer,Sales")]
        public async Task<ActionResult<DesignDto>> GetDesign(string id)
        {
            var design = await _designService.GetDesignByIdAsync(id);
            if (design == null) return NotFound();
            return Ok(design);
        }

        // API: POST api/Designs (ĐÃ NÂNG CẤP)
        [HttpPost]
        public async Task<ActionResult<DesignDto>> PostDesign(CreateDesignDto createDto)
        {
            var design = _mapper.Map<Design>(createDto);
            try
            {
                var createdDesign = await _designService.CreateDesignAsync(design);
                return CreatedAtAction(nameof(GetDesign), new { id = createdDesign.DesignID }, createdDesign);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // API: PUT api/Designs/5 (ĐÃ NÂNG CẤP)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDesign(string id, UpdateDesignDto updateDto)
        {
            // Cần inject ApplicationDbContext vào controller này để dùng FindAsync
            // private readonly ApplicationDbContext _context;
            // public DesignsController(IDesignService designService, ApplicationDbContext context, IMapper mapper) ...

            var design = await _context.Designs.FindAsync(id); // Giả sử đã inject _context
            if (design == null)
            {
                return NotFound();
            }
            
            _mapper.Map(updateDto, design);

            try
            {
                var result = await _designService.UpdateDesignAsync(id, design);
                if (!result) return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            return NoContent();
        }

        // DELETE: api/Designs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDesign(string id)
        {
            var result = await _designService.DeleteDesignAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}