using Microsoft.AspNetCore.Mvc;
using DecalXeAPI.Services.Interfaces;
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.Services.Helpers;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DesignTemplateItemsController : ControllerBase
    {
        private readonly IDesignTemplateItemService _designTemplateItemService;

        public DesignTemplateItemsController(IDesignTemplateItemService designTemplateItemService)
        {
            _designTemplateItemService = designTemplateItemService;
        }

        /// <summary>
        /// Lấy danh sách tất cả template items
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DesignTemplateItemDto>>> GetAllItems()
        {
            try
            {
                var items = await _designTemplateItemService.GetAllAsync();
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy danh sách template items.", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy template item theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<DesignTemplateItemDto>> GetItem(string id)
        {
            try
            {
                var item = await _designTemplateItemService.GetByIdAsync(id);
                if (item == null)
                {
                    return NotFound(new { message = $"Không tìm thấy template item với ID: {id}" });
                }
                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy template item.", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách template items theo design ID
        /// </summary>
        [HttpGet("by-design/{designId}")]
        public async Task<ActionResult<IEnumerable<DesignTemplateItemDto>>> GetItemsByDesign(string designId)
        {
            try
            {
                var items = await _designTemplateItemService.GetByDesignIdAsync(designId);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy template items theo design.", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách template items theo vị trí đặt
        /// </summary>
        [HttpGet("by-placement/{placementPosition}")]
        public async Task<ActionResult<IEnumerable<DesignTemplateItemDto>>> GetItemsByPlacement(VehiclePart placementPosition)
        {
            try
            {
                var items = await _designTemplateItemService.GetByPlacementPositionAsync(placementPosition);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy template items theo vị trí.", error = ex.Message });
            }
        }

        /// <summary>
        /// Tạo template item mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<DesignTemplateItemDto>> CreateItem(CreateDesignTemplateItemDto createDto)
        {
            try
            {
                var item = await _designTemplateItemService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetItem), new { id = item.Id }, item);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi tạo template item.", error = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật template item
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<DesignTemplateItemDto>> UpdateItem(string id, UpdateDesignTemplateItemDto updateDto)
        {
            try
            {
                var item = await _designTemplateItemService.UpdateAsync(id, updateDto);
                if (item == null)
                {
                    return NotFound(new { message = $"Không tìm thấy template item với ID: {id}" });
                }
                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi cập nhật template item.", error = ex.Message });
            }
        }

        /// <summary>
        /// Xóa template item
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteItem(string id)
        {
            try
            {
                var result = await _designTemplateItemService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"Không tìm thấy template item với ID: {id}" });
                }
                return Ok(new { message = "Đã xóa template item thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi xóa template item.", error = ex.Message });
            }
        }

        /// <summary>
        /// Kiểm tra template item có tồn tại không
        /// </summary>
        [HttpGet("{id}/exists")]
        public async Task<ActionResult<bool>> CheckItemExists(string id)
        {
            try
            {
                var exists = await _designTemplateItemService.ExistsAsync(id);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi kiểm tra template item.", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả vị trí đặt decal với mô tả
        /// </summary>
        [HttpGet("vehicle-parts")]
        public ActionResult<Dictionary<VehiclePart, string>> GetVehicleParts()
        {
            try
            {
                var vehicleParts = VehiclePartHelper.GetAllWithDescriptions();
                return Ok(vehicleParts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy danh sách vị trí xe.", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy display order tiếp theo cho design
        /// </summary>
        [HttpGet("next-display-order/{designId}")]
        public async Task<ActionResult<int>> GetNextDisplayOrder(string designId)
        {
            try
            {
                var nextOrder = await _designTemplateItemService.GetNextDisplayOrderForDesignAsync(designId);
                return Ok(nextOrder);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy display order.", error = ex.Message });
            }
        }
    }
}
