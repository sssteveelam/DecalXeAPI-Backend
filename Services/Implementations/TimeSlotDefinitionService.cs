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

namespace DecalXeAPI.Services.Implementations
{
    public class TimeSlotDefinitionService : ITimeSlotDefinitionService // <-- Kế thừa từ ITimeSlotDefinitionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<TimeSlotDefinitionService> _logger;

        public TimeSlotDefinitionService(ApplicationDbContext context, IMapper mapper, ILogger<TimeSlotDefinitionService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<TimeSlotDefinitionDto>> GetTimeSlotDefinitionsAsync()
        {
            _logger.LogInformation("Lấy danh sách định nghĩa khung giờ.");
            var timeSlotDefinitions = await _context.TimeSlotDefinitions.ToListAsync();
            var timeSlotDefinitionDtos = _mapper.Map<List<TimeSlotDefinitionDto>>(timeSlotDefinitions);
            return timeSlotDefinitionDtos;
        }

        public async Task<TimeSlotDefinitionDto?> GetTimeSlotDefinitionByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy định nghĩa khung giờ với ID: {SlotDefID}", id);
            var timeSlotDefinition = await _context.TimeSlotDefinitions.FindAsync(id);

            if (timeSlotDefinition == null)
            {
                _logger.LogWarning("Không tìm thấy định nghĩa khung giờ với ID: {SlotDefID}", id);
                return null;
            }

            var timeSlotDefinitionDto = _mapper.Map<TimeSlotDefinitionDto>(timeSlotDefinition);
            _logger.LogInformation("Đã trả về định nghĩa khung giờ với ID: {SlotDefID}", id);
            return timeSlotDefinitionDto;
        }

        public async Task<TimeSlotDefinitionDto> CreateTimeSlotDefinitionAsync(TimeSlotDefinition timeSlotDefinition)
        {
            _logger.LogInformation("Yêu cầu tạo định nghĩa khung giờ mới: {StartTime} - {EndTime}", timeSlotDefinition.StartTime, timeSlotDefinition.EndTime);
            _context.TimeSlotDefinitions.Add(timeSlotDefinition);
            await _context.SaveChangesAsync();

            var timeSlotDefinitionDto = _mapper.Map<TimeSlotDefinitionDto>(timeSlotDefinition);
            _logger.LogInformation("Đã tạo định nghĩa khung giờ mới với ID: {SlotDefID}", timeSlotDefinition.SlotDefID);
            return timeSlotDefinitionDto;
        }

        public async Task<bool> UpdateTimeSlotDefinitionAsync(string id, TimeSlotDefinition timeSlotDefinition)
        {
            _logger.LogInformation("Yêu cầu cập nhật định nghĩa khung giờ với ID: {SlotDefID}", id);

            if (id != timeSlotDefinition.SlotDefID)
            {
                _logger.LogWarning("ID trong tham số ({Id}) không khớp với SlotDefID trong body ({SlotDefIDBody})", id, timeSlotDefinition.SlotDefID);
                return false;
            }

            if (!await TimeSlotDefinitionExistsAsync(id))
            {
                _logger.LogWarning("Không tìm thấy định nghĩa khung giờ để cập nhật với ID: {SlotDefID}", id);
                return false;
            }

            _context.Entry(timeSlotDefinition).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật định nghĩa khung giờ với ID: {SlotDefID}", id);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật định nghĩa khung giờ với ID: {SlotDefID}", id);
                throw;
            }
        }

        public async Task<bool> DeleteTimeSlotDefinitionAsync(string id)
        {
            _logger.LogInformation("Yêu cầu xóa định nghĩa khung giờ với ID: {SlotDefID}", id);
            var timeSlotDefinition = await _context.TimeSlotDefinitions.FindAsync(id);
            if (timeSlotDefinition == null)
            {
                _logger.LogWarning("Không tìm thấy định nghĩa khung giờ để xóa với ID: {SlotDefID}", id);
                return false;
            }

            _context.TimeSlotDefinitions.Remove(timeSlotDefinition);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa định nghĩa khung giờ với ID: {SlotDefID}", id);
            return true;
        }

        // --- HÀM HỖ TRỢ: KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (PUBLIC CHO INTERFACE) ---
        public async Task<bool> TimeSlotDefinitionExistsAsync(string id)
        {
            return await _context.TimeSlotDefinitions.AnyAsync(e => e.SlotDefID == id);
        }
    }
}