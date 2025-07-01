// DecalXeAPI/Controllers/VehicleBrandsController.cs
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
    public class VehicleBrandsController : ControllerBase
    {
        private readonly IVehicleBrandService _brandService;

        public VehicleBrandsController(IVehicleBrandService brandService)
        {
            _brandService = brandService;
        }

        // GET: api/VehicleBrands
        [HttpGet]
        [AllowAnonymous] // Ai cũng có thể xem danh sách hãng xe
        public async Task<ActionResult<IEnumerable<VehicleBrandDto>>> GetVehicleBrands()
        {
            var brands = await _brandService.GetAllBrandsAsync();
            return Ok(brands);
        }

        // GET: api/VehicleBrands/5
        [HttpGet("{id}")]
        [AllowAnonymous] // Ai cũng có thể xem chi tiết hãng xe
        public async Task<ActionResult<VehicleBrandDto>> GetVehicleBrand(string id)
        {
            var brand = await _brandService.GetBrandByIdAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            return Ok(brand);
        }

        // POST: api/VehicleBrands
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")] // Chỉ Admin, Manager được tạo
        public async Task<ActionResult<VehicleBrandDto>> PostVehicleBrand(VehicleBrand brand)
        {
            var createdBrand = await _brandService.CreateBrandAsync(brand);
            return CreatedAtAction(nameof(GetVehicleBrand), new { id = createdBrand.BrandID }, createdBrand);
        }

        // PUT: api/VehicleBrands/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")] // Chỉ Admin, Manager được sửa
        public async Task<IActionResult> PutVehicleBrand(string id, VehicleBrand brand)
        {
            if (id != brand.BrandID)
            {
                return BadRequest();
            }
            var result = await _brandService.UpdateBrandAsync(id, brand);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        // DELETE: api/VehicleBrands/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")] // Chỉ Admin, Manager được xóa
        public async Task<IActionResult> DeleteVehicleBrand(string id)
        {
            var result = await _brandService.DeleteBrandAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}