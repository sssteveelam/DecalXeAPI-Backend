// DecalXeAPI/Controllers/TechLaborPricesController.cs
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using AutoMapper;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")]
    public class TechLaborPricesController : ControllerBase
    {
        private readonly ITechLaborPriceService _priceService;
        private readonly IMapper _mapper;

        public TechLaborPricesController(ITechLaborPriceService priceService, IMapper mapper)
        {
            _priceService = priceService;
            _mapper = mapper;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _priceService.GetAllAsync());
        }

        [HttpGet("{serviceId}/{vehicleModelId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(string serviceId, string vehicleModelId)
        {
            var price = await _priceService.GetByIdAsync(serviceId, vehicleModelId);
            if (price == null) return NotFound();
            return Ok(price);
        }

        // API: POST /api/TechLaborPrices (ĐÃ NÂNG CẤP)
        [HttpPost]
        public async Task<IActionResult> Create(CreateTechLaborPriceDto createDto)
        {
            var techLaborPrice = _mapper.Map<TechLaborPrice>(createDto);
            try
            {
                var createdPrice = await _priceService.CreateAsync(techLaborPrice);
                return CreatedAtAction(nameof(GetById), new { serviceId = createdPrice.ServiceID, vehicleModelId = createdPrice.VehicleModelID }, createdPrice);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // API: PUT /api/TechLaborPrices/{...} (ĐÃ NÂNG CẤP)
        [HttpPut("{serviceId}/{vehicleModelId}")]
        public async Task<IActionResult> Update(string serviceId, string vehicleModelId, UpdateTechLaborPriceDto updateDto)
        {
            // Logic cập nhật giờ sẽ chỉ nhận giá mới, an toàn hơn
            var updatedPrice = await _priceService.UpdateAsync(serviceId, vehicleModelId, updateDto.LaborPrice);
            if (updatedPrice == null) return NotFound();
            return Ok(updatedPrice);
        }

        [HttpDelete("{serviceId}/{vehicleModelId}")]
        public async Task<IActionResult> Delete(string serviceId, string vehicleModelId)
        {
            var success = await _priceService.DeleteAsync(serviceId, vehicleModelId);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}