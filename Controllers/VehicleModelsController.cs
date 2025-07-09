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
        public VehicleModelsController(IVehicleModelService modelService, IMapper mapper, ApplicationDbContext context)
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
        
        // --- API MỚI ĐỂ QUẢN LÝ DECALTYPE TƯƠNG THÍCH CHO VEHICLEMODEL ---

        /// <summary>
        /// Lấy danh sách các loại decal tương thích với một mẫu xe cụ thể.
        /// </summary>
        [HttpGet("{modelId}/decaltypes")]
        [AllowAnonymous] // Cho phép ai cũng có thể xem để tiện cho việc hiển thị ở frontend
        public async Task<ActionResult<IEnumerable<DecalTypeDto>>> GetCompatibleDecalTypes(string modelId)
        {
            var decalTypes = await _modelService.GetCompatibleDecalTypesAsync(modelId);
            return Ok(decalTypes);
        }

        /// <summary>
        /// Gán một loại decal là tương thích với một mẫu xe.
        /// </summary>
        [HttpPost("{modelId}/decaltypes/{decalTypeId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AssignDecalTypeToVehicle(string modelId, string decalTypeId)
        {
            var (success, errorMessage) = await _modelService.AssignDecalTypeToVehicleAsync(modelId, decalTypeId);
            if (!success)
            {
                return BadRequest(new { message = errorMessage });
            }
            return Ok(new { message = "Gán loại decal cho mẫu xe thành công." });
        }

        /// <summary>
        /// Gỡ (xóa) liên kết tương thích giữa một loại decal và một mẫu xe.
        /// </summary>
        [HttpDelete("{modelId}/decaltypes/{decalTypeId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UnassignDecalTypeFromVehicle(string modelId, string decalTypeId)
        {
            var (success, errorMessage) = await _modelService.UnassignDecalTypeFromVehicleAsync(modelId, decalTypeId);
            if (!success)
            {
                return BadRequest(new { message = errorMessage });
            }
            return NoContent();
        }


    }
}