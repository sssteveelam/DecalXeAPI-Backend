// DecalXeAPI/Controllers/VehicleModelsController.cs
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
    public class VehicleModelsController : ControllerBase
    {
        private readonly IVehicleModelService _modelService;

        public VehicleModelsController(IVehicleModelService modelService)
        {
            _modelService = modelService;
        }

        // GET: api/VehicleModels
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<VehicleModelDto>>> GetVehicleModels()
        {
            var models = await _modelService.GetAllModelsAsync();
            return Ok(models);
        }

        // GET: api/VehicleModels/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<VehicleModelDto>> GetVehicleModel(string id)
        {
            var model = await _modelService.GetModelByIdAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            return Ok(model);
        }

        // POST: api/VehicleModels
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<VehicleModelDto>> PostVehicleModel(VehicleModel model)
        {
            var (createdModelDto, errorMessage) = await _modelService.CreateModelAsync(model);
            if (createdModelDto == null)
            {
                return BadRequest(errorMessage);
            }
            return CreatedAtAction(nameof(GetVehicleModel), new { id = createdModelDto.ModelID }, createdModelDto);
        }

        // PUT: api/VehicleModels/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> PutVehicleModel(string id, VehicleModel model)
        {
            if (id != model.ModelID)
            {
                return BadRequest("ID không khớp.");
            }
            var (success, errorMessage) = await _modelService.UpdateModelAsync(id, model);
            if (!success)
            {
                // Nếu có thông báo lỗi cụ thể thì trả về BadRequest, ngược lại là NotFound
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    return BadRequest(errorMessage);
                }
                return NotFound();
            }
            return NoContent();
        }

        // DELETE: api/VehicleModels/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteVehicleModel(string id)
        {
            var result = await _modelService.DeleteModelAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}