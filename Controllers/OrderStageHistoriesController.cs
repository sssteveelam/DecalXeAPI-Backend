using Microsoft.AspNetCore.Mvc;
using DecalXeAPI.Services.Interfaces;
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.Services.Helpers;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderStageHistoriesController : ControllerBase
    {
        private readonly IOrderStageHistoryService _orderStageHistoryService;

        public OrderStageHistoriesController(IOrderStageHistoryService orderStageHistoryService)
        {
            _orderStageHistoryService = orderStageHistoryService;
        }

        /// <summary>
        /// Lấy danh sách tất cả lịch sử giai đoạn
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderStageHistoryDto>>> GetAllHistories()
        {
            try
            {
                var histories = await _orderStageHistoryService.GetAllAsync();
                return Ok(histories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy danh sách lịch sử giai đoạn.", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy lịch sử giai đoạn theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderStageHistoryDto>> GetHistory(string id)
        {
            try
            {
                var history = await _orderStageHistoryService.GetByIdAsync(id);
                if (history == null)
                {
                    return NotFound(new { message = $"Không tìm thấy lịch sử giai đoạn với ID: {id}" });
                }
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy lịch sử giai đoạn.", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy lịch sử giai đoạn theo Order ID
        /// </summary>
        [HttpGet("by-order/{orderId}")]
        public async Task<ActionResult<IEnumerable<OrderStageHistoryDto>>> GetHistoriesByOrder(string orderId)
        {
            try
            {
                var histories = await _orderStageHistoryService.GetByOrderIdAsync(orderId);
                return Ok(histories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy lịch sử giai đoạn theo đơn hàng.", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy lịch sử giai đoạn theo Stage
        /// </summary>
        [HttpGet("by-stage/{stage}")]
        public async Task<ActionResult<IEnumerable<OrderStageHistoryDto>>> GetHistoriesByStage(OrderStage stage)
        {
            try
            {
                var histories = await _orderStageHistoryService.GetByStageAsync(stage);
                return Ok(histories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy lịch sử giai đoạn theo stage.", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy lịch sử giai đoạn mới nhất của đơn hàng
        /// </summary>
        [HttpGet("latest-by-order/{orderId}")]
        public async Task<ActionResult<OrderStageHistoryDto>> GetLatestHistoryByOrder(string orderId)
        {
            try
            {
                var history = await _orderStageHistoryService.GetLatestByOrderIdAsync(orderId);
                if (history == null)
                {
                    return NotFound(new { message = $"Không tìm thấy lịch sử giai đoạn cho đơn hàng: {orderId}" });
                }
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy lịch sử giai đoạn mới nhất.", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy giai đoạn hiện tại của đơn hàng
        /// </summary>
        [HttpGet("current-stage/{orderId}")]
        public async Task<ActionResult<object>> GetCurrentStage(string orderId)
        {
            try
            {
                var currentStage = await _orderStageHistoryService.GetCurrentStageAsync(orderId);
                if (currentStage == null)
                {
                    return NotFound(new { message = $"Đơn hàng {orderId} chưa có lịch sử giai đoạn." });
                }

                return Ok(new
                {
                    stage = currentStage,
                    stageDescription = OrderStageHelper.GetDescription(currentStage.Value),
                    completionPercentage = OrderStageHelper.GetCompletionPercentage(currentStage.Value)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy giai đoạn hiện tại.", error = ex.Message });
            }
        }

        /// <summary>
        /// Tạo lịch sử giai đoạn mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<OrderStageHistoryDto>> CreateHistory(CreateOrderStageHistoryDto createDto)
        {
            try
            {
                var history = await _orderStageHistoryService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetHistory), new { id = history.OrderStageHistoryID }, history);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi tạo lịch sử giai đoạn.", error = ex.Message });
            }
        }

        /// <summary>
        /// Chuyển đơn hàng sang giai đoạn tiếp theo
        /// </summary>
        [HttpPost("transition-next/{orderId}")]
        public async Task<ActionResult<OrderStageHistoryDto>> TransitionToNextStage(
            string orderId, 
            [FromBody] TransitionStageDto? transitionDto = null)
        {
            try
            {
                var history = await _orderStageHistoryService.TransitionToNextStageAsync(
                    orderId, 
                    transitionDto?.EmployeeId, 
                    transitionDto?.Notes
                );
                return Ok(history);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi chuyển giai đoạn.", error = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật lịch sử giai đoạn
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<OrderStageHistoryDto>> UpdateHistory(string id, UpdateOrderStageHistoryDto updateDto)
        {
            try
            {
                var history = await _orderStageHistoryService.UpdateAsync(id, updateDto);
                if (history == null)
                {
                    return NotFound(new { message = $"Không tìm thấy lịch sử giai đoạn với ID: {id}" });
                }
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi cập nhật lịch sử giai đoạn.", error = ex.Message });
            }
        }

        /// <summary>
        /// Xóa lịch sử giai đoạn
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteHistory(string id)
        {
            try
            {
                var result = await _orderStageHistoryService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"Không tìm thấy lịch sử giai đoạn với ID: {id}" });
                }
                return Ok(new { message = "Đã xóa lịch sử giai đoạn thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi xóa lịch sử giai đoạn.", error = ex.Message });
            }
        }

        /// <summary>
        /// Kiểm tra có thể chuyển sang giai đoạn mới không
        /// </summary>
        [HttpGet("can-transition/{orderId}/{newStage}")]
        public async Task<ActionResult<bool>> CanTransitionToStage(string orderId, OrderStage newStage)
        {
            try
            {
                var canTransition = await _orderStageHistoryService.CanTransitionToStageAsync(orderId, newStage);
                return Ok(canTransition);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi kiểm tra chuyển giai đoạn.", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả giai đoạn với mô tả
        /// </summary>
        [HttpGet("stages")]
        public ActionResult<Dictionary<OrderStage, string>> GetAllStages()
        {
            try
            {
                var stages = OrderStageHelper.GetAllWithDescriptions();
                return Ok(stages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy danh sách giai đoạn.", error = ex.Message });
            }
        }

        /// <summary>
        /// Kiểm tra lịch sử giai đoạn có tồn tại không
        /// </summary>
        [HttpGet("{id}/exists")]
        public async Task<ActionResult<bool>> CheckHistoryExists(string id)
        {
            try
            {
                var exists = await _orderStageHistoryService.ExistsAsync(id);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi kiểm tra lịch sử giai đoạn.", error = ex.Message });
            }
        }
    }

    /// <summary>
    /// DTO để chuyển giai đoạn
    /// </summary>
    public class TransitionStageDto
    {
        public string? EmployeeId { get; set; }
        public string? Notes { get; set; }
    }
}
