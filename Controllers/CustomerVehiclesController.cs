using Microsoft.AspNetCore.Mvc;
using DecalXeAPI.Services.Interfaces;
using DecalXeAPI.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerVehiclesController : ControllerBase
    {
        private readonly ICustomerVehicleService _customerVehicleService;

        public CustomerVehiclesController(ICustomerVehicleService customerVehicleService)
        {
            _customerVehicleService = customerVehicleService;
        }

        /// <summary>
        /// Lấy danh sách tất cả xe khách hàng
        /// </summary>
        /// <returns>Danh sách xe khách hàng</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerVehicleDto>>> GetAllVehicles()
        {
            try
            {
                var vehicles = await _customerVehicleService.GetAllAsync();
                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy danh sách xe.", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thông tin xe theo ID
        /// </summary>
        /// <param name="id">ID của xe</param>
        /// <returns>Thông tin xe</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerVehicleDto>> GetVehicle(string id)
        {
            try
            {
                var vehicle = await _customerVehicleService.GetByIdAsync(id);
                if (vehicle == null)
                {
                    return NotFound(new { message = $"Không tìm thấy xe với ID: {id}" });
                }
                return Ok(vehicle);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy thông tin xe.", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thông tin xe theo biển số
        /// </summary>
        /// <param name="licensePlate">Biển số xe</param>
        /// <returns>Thông tin xe</returns>
        [HttpGet("by-license-plate/{licensePlate}")]
        public async Task<ActionResult<CustomerVehicleDto>> GetVehicleByLicensePlate(string licensePlate)
        {
            try
            {
                var vehicle = await _customerVehicleService.GetByLicensePlateAsync(licensePlate);
                if (vehicle == null)
                {
                    return NotFound(new { message = $"Không tìm thấy xe với biển số: {licensePlate}" });
                }
                return Ok(vehicle);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy thông tin xe.", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách xe của khách hàng
        /// </summary>
        /// <param name="customerId">ID khách hàng</param>
        /// <returns>Danh sách xe của khách hàng</returns>
        [HttpGet("by-customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<CustomerVehicleDto>>> GetVehiclesByCustomer(string customerId)
        {
            try
            {
                var vehicles = await _customerVehicleService.GetByCustomerIdAsync(customerId);
                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy danh sách xe khách hàng.", error = ex.Message });
            }
        }

        /// <summary>
        /// Tạo xe mới
        /// </summary>
        /// <param name="createDto">Thông tin xe cần tạo</param>
        /// <returns>Thông tin xe đã tạo</returns>
        [HttpPost]
        public async Task<ActionResult<CustomerVehicleDto>> CreateVehicle(CreateCustomerVehicleDto createDto)
        {
            try
            {
                var vehicle = await _customerVehicleService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.VehicleID }, vehicle);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi tạo xe.", error = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật thông tin xe
        /// </summary>
        /// <param name="id">ID của xe</param>
        /// <param name="updateDto">Thông tin cần cập nhật</param>
        /// <returns>Thông tin xe đã cập nhật</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<CustomerVehicleDto>> UpdateVehicle(string id, UpdateCustomerVehicleDto updateDto)
        {
            try
            {
                var vehicle = await _customerVehicleService.UpdateAsync(id, updateDto);
                if (vehicle == null)
                {
                    return NotFound(new { message = $"Không tìm thấy xe với ID: {id}" });
                }
                return Ok(vehicle);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi cập nhật xe.", error = ex.Message });
            }
        }

        /// <summary>
        /// Xóa xe
        /// </summary>
        /// <param name="id">ID của xe</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteVehicle(string id)
        {
            try
            {
                var result = await _customerVehicleService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"Không tìm thấy xe với ID: {id}" });
                }
                return Ok(new { message = "Đã xóa xe thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi xóa xe.", error = ex.Message });
            }
        }

        /// <summary>
        /// Kiểm tra xe có tồn tại không
        /// </summary>
        /// <param name="id">ID của xe</param>
        /// <returns>True nếu tồn tại, False nếu không</returns>
        [HttpGet("{id}/exists")]
        public async Task<ActionResult<bool>> CheckVehicleExists(string id)
        {
            try
            {
                var exists = await _customerVehicleService.ExistsAsync(id);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi kiểm tra xe.", error = ex.Message });
            }
        }

        /// <summary>
        /// Kiểm tra biển số có tồn tại không
        /// </summary>
        /// <param name="licensePlate">Biển số xe</param>
        /// <returns>True nếu tồn tại, False nếu không</returns>
        [HttpGet("license-plate/{licensePlate}/exists")]
        public async Task<ActionResult<bool>> CheckLicensePlateExists(string licensePlate)
        {
            try
            {
                var exists = await _customerVehicleService.LicensePlateExistsAsync(licensePlate);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi kiểm tra biển số.", error = ex.Message });
            }
        }

        /// <summary>
        /// Kiểm tra số khung có tồn tại không
        /// </summary>
        /// <param name="chassisNumber">Số khung xe</param>
        /// <returns>True nếu tồn tại, False nếu không</returns>
        [HttpGet("chassis/{chassisNumber}/exists")]
        public async Task<ActionResult<bool>> CheckChassisNumberExists(string chassisNumber)
        {
            try
            {
                var exists = await _customerVehicleService.ChassisNumberExistsAsync(chassisNumber);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi kiểm tra số khung.", error = ex.Message });
            }
        }
    }
}
