// DecalXeAPI/Controllers/VehicleModelsController.cs
using Microsoft.AspNetCore.Mvc;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using DecalXeAPI.Data;
using AutoMapper;   
namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleModelsController : ControllerBase
    {
        private readonly IVehicleModelService _modelService;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context; // Cần cho Put
        public VehicleModelsController(IVehicleModelService modelService, IMapper mapper, ApplicationDbContext context )
        {
            _modelService = modelService;
            _mapper = mapper;
            _context = context; // Cần cho Put
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
        // API: POST api/VehicleModels (ĐÃ NÂNG CẤP)
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<VehicleModelDto>> PostVehicleModel(CreateVehicleModelDto createDto)
        {
            var model = _mapper.Map<VehicleModel>(createDto);
            var (createdModelDto, errorMessage) = await _modelService.CreateModelAsync(model);
            if (createdModelDto == null)
            {
                return BadRequest(errorMessage);
            }
            return CreatedAtAction(nameof(GetVehicleModel), new { id = createdModelDto.ModelID }, createdModelDto);
        }

        // API: PUT api/VehicleModels/{id} (ĐÃ NÂNG CẤP)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> PutVehicleModel(string id, UpdateVehicleModelDto updateDto)
        {
            var model = await _context.VehicleModels.FindAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            
            _mapper.Map(updateDto, model);

            var (success, errorMessage) = await _modelService.UpdateModelAsync(id, model);
            if (!success)
            {
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