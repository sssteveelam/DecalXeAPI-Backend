// DecalXeAPI/Controllers/ServiceVehicleModelProductsController.cs
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using DecalXeAPI.Data;
using AutoMapper;


namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")]
    public class ServiceVehicleModelProductsController : ControllerBase
    {
        private readonly IServiceVehicleModelProductService _linkService;
        private readonly IMapper _mapper;
        public ServiceVehicleModelProductsController(IServiceVehicleModelProductService linkService, IMapper mapper)
        {
            _linkService = linkService;
            _mapper = mapper;
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

        // API: POST /api/ServiceVehicleModelProducts (ĐÃ NÂNG CẤP)
        [HttpPost]
        public async Task<IActionResult> Create(CreateServiceVehicleModelProductDto createDto)
        {
            var link = _mapper.Map<ServiceVehicleModelProduct>(createDto);
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

        // API: PUT /api/ServiceVehicleModelProducts/{...} (ĐÃ NÂNG CẤP)
        [HttpPut("{serviceId}/{vehicleModelId}/{productId}")]
        public async Task<IActionResult> Update(string serviceId, string vehicleModelId, string productId, UpdateServiceVehicleModelProductDto updateDto)
        {
            var updatedLink = await _linkService.UpdateQuantityAsync(serviceId, vehicleModelId, productId, updateDto.Quantity);
            if (updatedLink == null) return NotFound();
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