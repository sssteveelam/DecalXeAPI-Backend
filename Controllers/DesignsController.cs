// DecalXeAPI/Controllers/DesignsController.cs
using Microsoft.AspNetCore.Mvc;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,Designer")]
    public class DesignsController : ControllerBase
    {
        private readonly IDesignService _designService;

        public DesignsController(IDesignService designService)
        {
            _designService = designService;
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

        // POST: api/Designs
        [HttpPost]
        public async Task<ActionResult<DesignDto>> PostDesign(Design design)
        {
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

        // PUT: api/Designs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDesign(string id, Design design)
        {
            if (id != design.DesignID) return BadRequest();

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