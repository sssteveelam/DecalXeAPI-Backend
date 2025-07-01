// DecalXeAPI/Controllers/VehicleModelDecalTemplatesController.cs
using Microsoft.AspNetCore.Mvc;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")]
    public class VehicleModelDecalTemplatesController : ControllerBase
    {
        private readonly IVehicleModelDecalTemplateService _linkService;

        public VehicleModelDecalTemplatesController(IVehicleModelDecalTemplateService linkService)
        {
            _linkService = linkService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehicleModelDecalTemplateDto>>> GetAllLinks()
        {
            return Ok(await _linkService.GetAllAsync());
        }

        [HttpPost]
        public async Task<ActionResult<VehicleModelDecalTemplateDto>> CreateLink(VehicleModelDecalTemplate link)
        {
            var (createdLink, error) = await _linkService.CreateAsync(link);
            if (createdLink == null)
            {
                return BadRequest(error);
            }
            return Ok(createdLink); // Trả về 200 OK cùng với object đã tạo
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLink(string id)
        {
            var result = await _linkService.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}