// DecalXeAPI/Controllers/TechLaborPricesController.cs
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")]
    public class TechLaborPricesController : ControllerBase
    {
        private readonly ITechLaborPriceService _priceService;

        public TechLaborPricesController(ITechLaborPriceService priceService)
        {
            _priceService = priceService;
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

        [HttpPost]
        public async Task<IActionResult> Create(TechLaborPrice techLaborPrice)
        {
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

        [HttpPut("{serviceId}/{vehicleModelId}")]
        public async Task<IActionResult> Update(string serviceId, string vehicleModelId, [FromBody] decimal newPrice)
        {
            var updatedPrice = await _priceService.UpdateAsync(serviceId, vehicleModelId, newPrice);
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