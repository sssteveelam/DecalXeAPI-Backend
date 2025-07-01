// DecalXeAPI/Controllers/ServiceVehicleModelProductsController.cs
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
    public class ServiceVehicleModelProductsController : ControllerBase
    {
        private readonly IServiceVehicleModelProductService _linkService;

        public ServiceVehicleModelProductsController(IServiceVehicleModelProductService linkService)
        {
            _linkService = linkService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _linkService.GetAllAsync());
        }

        [HttpGet("{serviceId}/{vehicleModelId}/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(string serviceId, string vehicleModelId, string productId)
        {
            var link = await _linkService.GetByIdAsync(serviceId, vehicleModelId, productId);
            if (link == null) return NotFound();
            return Ok(link);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ServiceVehicleModelProduct link)
        {
            try
            {
                var createdLink = await _linkService.CreateAsync(link);
                return CreatedAtAction(nameof(GetById), new { serviceId = createdLink.ServiceID, vehicleModelId = createdLink.VehicleModelID, productId = createdLink.ProductID }, createdLink);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{serviceId}/{vehicleModelId}/{productId}")]
        public async Task<IActionResult> Update(string serviceId, string vehicleModelId, string productId, [FromBody] int newQuantity)
        {
            var updatedLink = await _linkService.UpdateQuantityAsync(serviceId, vehicleModelId, productId, newQuantity);
            if(updatedLink == null) return NotFound();
            return Ok(updatedLink);
        }

        [HttpDelete("{serviceId}/{vehicleModelId}/{productId}")]
        public async Task<IActionResult> Delete(string serviceId, string vehicleModelId, string productId)
        {
            var success = await _linkService.DeleteAsync(serviceId, vehicleModelId, productId);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}