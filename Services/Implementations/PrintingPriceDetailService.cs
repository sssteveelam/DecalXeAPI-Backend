// File: Services/Implementations/PrintingPriceDetailService.cs

using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.Services.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace DecalXeAPI.Services.Implementations // <-- ĐẢM BẢO NAMESPACE LÀ ĐÚNG
{
    public class PrintingPriceDetailService : IPrintingPriceDetailService // <-- ĐẢM BẢO TÊN LỚP LÀ ĐÚNG
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PrintingPriceDetailService> _logger;

        public PrintingPriceDetailService(ApplicationDbContext context, IMapper mapper, ILogger<PrintingPriceDetailService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<PrintingPriceDetailDto>> GetPrintingPriceDetailsAsync()
        {
            _logger.LogInformation("Lấy danh sách chi tiết giá in.");
            var printingPriceDetails = await _context.PrintingPriceDetails.Include(ppd => ppd.DecalService).ToListAsync();
            var printingPriceDetailDtos = _mapper.Map<List<PrintingPriceDetailDto>>(printingPriceDetails);
            return printingPriceDetailDtos;
        }

        public async Task<PrintingPriceDetailDto?> GetPrintingPriceDetailByIdAsync(string serviceId)
        {
            _logger.LogInformation("Yêu cầu lấy chi tiết giá in với ServiceID: {ServiceID}", serviceId);
            var printingPriceDetail = await _context.PrintingPriceDetails.Include(ppd => ppd.DecalService).FirstOrDefaultAsync(ppd => ppd.ServiceID == serviceId);

            if (printingPriceDetail == null)
            {
                _logger.LogWarning("Không tìm thấy chi tiết giá in với ServiceID: {ServiceID}", serviceId);
                return null;
            }

            var printingPriceDetailDto = _mapper.Map<PrintingPriceDetailDto>(printingPriceDetail);
            _logger.LogInformation("Đã trả về chi tiết giá in với ServiceID: {ServiceID}", serviceId);
            return printingPriceDetailDto;
        }

        public async Task<PrintingPriceDetailDto> CreatePrintingPriceDetailAsync(PrintingPriceDetail printingPriceDetail)
        {
            _logger.LogInformation("Yêu cầu tạo chi tiết giá in mới cho ServiceID: {ServiceID}", printingPriceDetail.ServiceID);

            if (!string.IsNullOrEmpty(printingPriceDetail.ServiceID) && !await DecalServiceExistsAsync(printingPriceDetail.ServiceID))
            {
                _logger.LogWarning("ServiceID không tồn tại khi tạo PrintingPriceDetail: {ServiceID}", printingPriceDetail.ServiceID);
                throw new ArgumentException("ServiceID không tồn tại.");
            }
            if (await PrintingPriceDetailExistsAsync(printingPriceDetail.ServiceID))
            {
                _logger.LogWarning("ServiceID đã có chi tiết giá in. Chỉ có thể có 1 chi tiết giá in/ServiceID: {ServiceID}", printingPriceDetail.ServiceID);
                throw new ArgumentException("ServiceID này đã có chi tiết giá in. Vui lòng cập nhật thay vì tạo mới.");
            }

            _context.PrintingPriceDetails.Add(printingPriceDetail);
            await _context.SaveChangesAsync();

            await _context.Entry(printingPriceDetail).Reference(ppd => ppd.DecalService).LoadAsync();

            var printingPriceDetailDto = _mapper.Map<PrintingPriceDetailDto>(printingPriceDetail);
            _logger.LogInformation("Đã tạo chi tiết giá in mới với ServiceID: {ServiceID}", printingPriceDetail.ServiceID);
            return printingPriceDetailDto;
        }

        public async Task<bool> UpdatePrintingPriceDetailAsync(string serviceId, PrintingPriceDetail printingPriceDetail)
        {
            _logger.LogInformation("Yêu cầu cập nhật chi tiết giá in với ServiceID: {ServiceID}", serviceId);

            if (serviceId != printingPriceDetail.ServiceID)
            {
                _logger.LogWarning("ID trong tham số ({Id}) không khớp với ServiceID trong body ({ServiceIDBody})", serviceId, printingPriceDetail.ServiceID);
                return false;
            }

            if (!await PrintingPriceDetailExistsAsync(serviceId))
            {
                _logger.LogWarning("Không tìm thấy chi tiết giá in để cập nhật với ServiceID: {ServiceID}", serviceId);
                return false;
            }

            _context.Entry(printingPriceDetail).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật chi tiết giá in với ServiceID: {ServiceID}", serviceId);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật chi tiết giá in với ServiceID: {ServiceID}", serviceId);
                throw;
            }
        }

        public async Task<bool> DeletePrintingPriceDetailAsync(string serviceId)
        {
            _logger.LogInformation("Yêu cầu xóa chi tiết giá in với ServiceID: {ServiceID}", serviceId);
            var printingPriceDetail = await _context.PrintingPriceDetails.FindAsync(serviceId);
            if (printingPriceDetail == null)
            {
                _logger.LogWarning("Không tìm thấy chi tiết giá in để xóa với ServiceID: {ServiceID}", serviceId);
                return false;
            }

            _context.PrintingPriceDetails.Remove(printingPriceDetail);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa chi tiết giá in với ServiceID: {ServiceID}", serviceId);
            return true;
        }

        public async Task<bool> PrintingPriceDetailExistsAsync(string serviceId)
        {
            return await _context.PrintingPriceDetails.AnyAsync(e => e.ServiceID == serviceId);
        }

        public async Task<bool> DecalServiceExistsAsync(string serviceId)
        {
            return await _context.DecalServices.AnyAsync(e => e.ServiceID == serviceId);
        }
    }
}