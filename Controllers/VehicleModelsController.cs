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
        private readonly ApplicationDbContext _context;
        private readonly IDecalTemplateService _decalTemplateService;

        public VehicleModelsController(IVehicleModelService modelService, IMapper mapper, ApplicationDbContext context, IDecalTemplateService decalTemplateService)
        {
            _modelService = modelService;
            _mapper = mapper;
            _context = context;
            _decalTemplateService = decalTemplateService;
        }

        // --- CÁC API QUẢN LÝ VEHICLE MODEL (GIỮ NGUYÊN) ---
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<VehicleModelDto>>> GetVehicleModels()
        {
            var models = await _modelService.GetAllModelsAsync();
            return Ok(models);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<VehicleModelDto>> GetVehicleModel(string id)
        {
            var model = await _modelService.GetModelByIdAsync(id);
            if (model == null) return NotFound();
            return Ok(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<VehicleModelDto>> PostVehicleModel(CreateVehicleModelDto createDto)
        {
            var model = _mapper.Map<VehicleModel>(createDto);
            var (createdModelDto, errorMessage) = await _modelService.CreateModelAsync(model);
            if (createdModelDto == null) return BadRequest(new { message = errorMessage });
            return CreatedAtAction(nameof(GetVehicleModel), new { id = createdModelDto.ModelID }, createdModelDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> PutVehicleModel(string id, UpdateVehicleModelDto updateDto)
        {
            var model = await _context.VehicleModels.FindAsync(id);
            if (model == null) return NotFound();
            _mapper.Map(updateDto, model);
            var (success, errorMessage) = await _modelService.UpdateModelAsync(id, model);
            if (!success)
            {
                if (!string.IsNullOrEmpty(errorMessage)) return BadRequest(new { message = errorMessage });
                return NotFound();
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteVehicleModel(string id)
        {
            var result = await _modelService.DeleteModelAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        // --- CÁC API MỚI ĐỂ QUẢN LÝ DECALTYPE TƯƠNG THÍCH VÀ GIÁ CẢ ---

        [HttpGet("{modelId}/decaltypes")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<VehicleModelDecalTypeDto>>> GetCompatibleDecalTypes(string modelId)
        {
            var decalTypes = await _modelService.GetCompatibleDecalTypesAsync(modelId);
            return Ok(decalTypes);
        }

        [HttpPost("{modelId}/decaltypes/{decalTypeId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<VehicleModelDecalTypeDto>> AssignDecalTypeToVehicle(string modelId, string decalTypeId, [FromBody] AssignDecalTypeToVehicleDto dto)
        {
            var (createdLink, errorMessage) = await _modelService.AssignDecalTypeToVehicleAsync(modelId, decalTypeId, dto.Price);
            if (createdLink == null)
            {
                return BadRequest(new { message = errorMessage });
            }
            return CreatedAtAction(nameof(GetCompatibleDecalTypes), new { modelId = modelId }, createdLink);
        }

        [HttpPut("{modelId}/decaltypes/{decalTypeId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<VehicleModelDecalTypeDto>> UpdateVehicleDecalTypePrice(string modelId, string decalTypeId, [FromBody] UpdateVehicleDecalTypePriceDto dto)
        {
            var (updatedLink, errorMessage) = await _modelService.UpdateVehicleDecalTypePriceAsync(modelId, decalTypeId, dto.NewPrice);
            if (updatedLink == null)
            {
                return BadRequest(new { message = errorMessage });
            }
            return Ok(updatedLink);
        }

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
        

        [HttpGet("{modelId}/templates")]
        [AllowAnonymous] // Cho phép tất cả mọi người xem
        public async Task<ActionResult<IEnumerable<DecalTemplateDto>>> GetTemplatesForVehicleModel(string modelId)
        {
            // Kiểm tra xem mẫu xe có tồn tại không để báo lỗi cho đẹp
            var modelExists = await _modelService.GetModelByIdAsync(modelId);
            if (modelExists == null)
            {
                return NotFound(new { message = "Không tìm thấy mẫu xe này." });
            }

            // Gọi service huynh đệ mình đã làm ở bước 1 để lấy danh sách template
            var templates = await _decalTemplateService.GetTemplatesByModelIdAsync(modelId);
            return Ok(templates);
        }
    }
}