using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data; // Vẫn cần DbContext cho các hàm Exists cơ bản
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using DecalXeAPI.Services.Interfaces; // Để sử dụng IPrintingPriceDetailService
using AutoMapper;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")] // Chỉ Admin, Manager có quyền quản lý giá in
    public class PrintingPriceDetailsController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Vẫn giữ để dùng các hàm Exists cơ bản
        private readonly IPrintingPriceDetailService _printingPriceDetailService; // Khai báo biến cho Service
        private readonly IMapper _mapper;
        private readonly ILogger<PrintingPriceDetailsController> _logger;

        public PrintingPriceDetailsController(ApplicationDbContext context, IPrintingPriceDetailService printingPriceDetailService, IMapper mapper, ILogger<PrintingPriceDetailsController> logger)
        {
            _context = context;
            _printingPriceDetailService = printingPriceDetailService;
            _mapper = mapper;
            _logger = logger;
        }

        // API: GET api/PrintingPriceDetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PrintingPriceDetailDto>>> GetPrintingPriceDetails()
        {
            _logger.LogInformation("Yêu cầu lấy danh sách chi tiết giá in.");
            var details = await _printingPriceDetailService.GetPrintingPriceDetailsAsync();
            return Ok(details);
        }

        // API: GET api/PrintingPriceDetails/{serviceId}
        [HttpGet("{serviceId}")]
        public async Task<ActionResult<PrintingPriceDetailDto>> GetPrintingPriceDetail(string serviceId)
        {
            _logger.LogInformation("Yêu cầu lấy chi tiết giá in với ServiceID: {ServiceID}", serviceId);
            var detailDto = await _printingPriceDetailService.GetPrintingPriceDetailByIdAsync(serviceId);

            if (detailDto == null)
            {
                _logger.LogWarning("Không tìm thấy chi tiết giá in với ServiceID: {ServiceID}", serviceId);
                return NotFound();
            }

            return Ok(detailDto);
        }

        // API: POST api/PrintingPriceDetails
        [HttpPost]
        public async Task<ActionResult<PrintingPriceDetailDto>> PostPrintingPriceDetail(PrintingPriceDetail printingPriceDetail)
        {
            _logger.LogInformation("Yêu cầu tạo chi tiết giá in mới cho ServiceID: {ServiceID}", printingPriceDetail.ServiceID);

            // Kiểm tra DecalServiceExists (FK) trước khi gọi Service
            if (!string.IsNullOrEmpty(printingPriceDetail.ServiceID) && !DecalServiceExists(printingPriceDetail.ServiceID))
            {
                return BadRequest("ServiceID không tồn tại.");
            }

            try
            {
                var createdDto = await _printingPriceDetailService.CreatePrintingPriceDetailAsync(printingPriceDetail);
                _logger.LogInformation("Đã tạo chi tiết giá in mới với ServiceID: {ServiceID}", createdDto.ServiceID);
                return CreatedAtAction(nameof(GetPrintingPriceDetail), new { serviceId = createdDto.ServiceID }, createdDto);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi tạo chi tiết giá in: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        // API: PUT api/PrintingPriceDetails/{serviceId}
        [HttpPut("{serviceId}")]
        public async Task<IActionResult> PutPrintingPriceDetail(string serviceId, PrintingPriceDetail printingPriceDetail)
        {
            _logger.LogInformation("Yêu cầu cập nhật chi tiết giá in với ServiceID: {ServiceID}", serviceId);
            if (serviceId != printingPriceDetail.ServiceID)
            {
                return BadRequest();
            }

            // Kiểm tra DecalServiceExists (FK) trước khi gọi Service
            if (!string.IsNullOrEmpty(printingPriceDetail.ServiceID) && !DecalServiceExists(printingPriceDetail.ServiceID))
            {
                return BadRequest("ServiceID không tồn tại.");
            }

            try
            {
                var success = await _printingPriceDetailService.UpdatePrintingPriceDetailAsync(serviceId, printingPriceDetail);

                if (!success)
                {
                    _logger.LogWarning("Không tìm thấy chi tiết giá in để cập nhật với ServiceID: {ServiceID}", serviceId);
                    return NotFound();
                }

                _logger.LogInformation("Đã cập nhật chi tiết giá in với ServiceID: {ServiceID}", serviceId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi cập nhật chi tiết giá in: {ErrorMessage}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PrintingPriceDetailExists(serviceId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // API: DELETE api/PrintingPriceDetails/{serviceId}
        [HttpDelete("{serviceId}")]
        public async Task<IActionResult> DeletePrintingPriceDetail(string serviceId)
        {
            _logger.LogInformation("Yêu cầu xóa chi tiết giá in với ServiceID: {ServiceID}", serviceId);
            var success = await _printingPriceDetailService.DeletePrintingPriceDetailAsync(serviceId);

            if (!success)
            {
                _logger.LogWarning("Không tìm thấy chi tiết giá in để xóa với ServiceID: {ServiceID}", serviceId);
                return NotFound();
            }

            return NoContent();
        }

        // --- HÀM HỖ TRỢ (PRIVATE): KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (Vẫn giữ ở Controller để kiểm tra FKs) ---
        private bool PrintingPriceDetailExists(string id) { return _context.PrintingPriceDetails.Any(e => e.ServiceID == id); }
        private bool DecalServiceExists(string id) { return _context.DecalServices.Any(e => e.ServiceID == id); }
    }
}