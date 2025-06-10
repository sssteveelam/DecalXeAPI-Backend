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
using System; // Để dùng ArgumentException

namespace DecalXeAPI.Services.Implementations
{
    public class TechnicianDailyScheduleService : ITechnicianDailyScheduleService // <-- Kế thừa từ ITechnicianDailyScheduleService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<TechnicianDailyScheduleService> _logger;

        public TechnicianDailyScheduleService(ApplicationDbContext context, IMapper mapper, ILogger<TechnicianDailyScheduleService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<TechnicianDailyScheduleDto>> GetTechnicianDailySchedulesAsync()
        {
            _logger.LogInformation("Lấy danh sách lịch làm việc hàng ngày của kỹ thuật viên.");
            var schedules = await _context.TechnicianDailySchedules.Include(tds => tds.Employee).ToListAsync();
            var scheduleDtos = _mapper.Map<List<TechnicianDailyScheduleDto>>(schedules);
            return scheduleDtos;
        }

        public async Task<TechnicianDailyScheduleDto?> GetTechnicianDailyScheduleByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy lịch làm việc hàng ngày với ID: {DailyScheduleID}", id);
            var schedule = await _context.TechnicianDailySchedules.Include(tds => tds.Employee).FirstOrDefaultAsync(tds => tds.DailyScheduleID == id);

            if (schedule == null)
            {
                _logger.LogWarning("Không tìm thấy lịch làm việc hàng ngày với ID: {DailyScheduleID}", id);
                return null;
            }

            var scheduleDto = _mapper.Map<TechnicianDailyScheduleDto>(schedule);
            _logger.LogInformation("Đã trả về lịch làm việc hàng ngày với ID: {DailyScheduleID}", id);
            return scheduleDto;
        }

        public async Task<TechnicianDailyScheduleDto> CreateTechnicianDailyScheduleAsync(TechnicianDailySchedule schedule)
        {
            _logger.LogInformation("Yêu cầu tạo lịch làm việc hàng ngày mới cho EmployeeID: {EmployeeID}, Ngày: {ScheduleDate}", schedule.EmployeeID, schedule.ScheduleDate);

            // Kiểm tra FKs
            if (!string.IsNullOrEmpty(schedule.EmployeeID) && !await EmployeeExistsAsync(schedule.EmployeeID))
            {
                _logger.LogWarning("EmployeeID không tồn tại khi tạo lịch: {EmployeeID}", schedule.EmployeeID);
                throw new ArgumentException("EmployeeID không tồn tại.");
            }

            _context.TechnicianDailySchedules.Add(schedule);
            await _context.SaveChangesAsync();

            await _context.Entry(schedule).Reference(tds => tds.Employee).LoadAsync();

            var scheduleDto = _mapper.Map<TechnicianDailyScheduleDto>(schedule);
            _logger.LogInformation("Đã tạo lịch làm việc hàng ngày mới với ID: {DailyScheduleID}", schedule.DailyScheduleID);
            return scheduleDto;
        }

        public async Task<bool> UpdateTechnicianDailyScheduleAsync(string id, TechnicianDailySchedule schedule)
        {
            _logger.LogInformation("Yêu cầu cập nhật lịch làm việc hàng ngày với ID: {DailyScheduleID}", id);

            if (id != schedule.DailyScheduleID)
            {
                _logger.LogWarning("ID trong tham số ({Id}) không khớp với DailyScheduleID trong body ({DailyScheduleIDBody})", id, schedule.DailyScheduleID);
                return false;
            }

            if (!await TechnicianDailyScheduleExistsAsync(id))
            {
                _logger.LogWarning("Không tìm thấy lịch làm việc hàng ngày để cập nhật với ID: {DailyScheduleID}", id);
                return false;
            }

            // Kiểm tra FKs
            if (!string.IsNullOrEmpty(schedule.EmployeeID) && !await EmployeeExistsAsync(schedule.EmployeeID))
            {
                _logger.LogWarning("EmployeeID không tồn tại khi cập nhật lịch: {EmployeeID}", schedule.EmployeeID);
                throw new ArgumentException("EmployeeID không tồn tại.");
            }

            _context.Entry(schedule).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật lịch làm việc hàng ngày với ID: {DailyScheduleID}", id);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật lịch làm việc hàng ngày với ID: {DailyScheduleID}", id);
                throw;
            }
        }

        public async Task<bool> DeleteTechnicianDailyScheduleAsync(string id)
        {
            _logger.LogInformation("Yêu cầu xóa lịch làm việc hàng ngày với ID: {DailyScheduleID}", id);
            var schedule = await _context.TechnicianDailySchedules.FindAsync(id);
            if (schedule == null)
            {
                _logger.LogWarning("Không tìm thấy lịch làm việc hàng ngày để xóa với ID: {DailyScheduleID}", id);
                return false;
            }

            _context.TechnicianDailySchedules.Remove(schedule);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa lịch làm việc hàng ngày với ID: {DailyScheduleID}", id);
            return true;
        }

        // --- HÀM HỖ TRỢ: KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (PUBLIC CHO INTERFACE) ---
        public async Task<bool> TechnicianDailyScheduleExistsAsync(string id)
        {
            return await _context.TechnicianDailySchedules.AnyAsync(e => e.DailyScheduleID == id);
        }

        public async Task<bool> EmployeeExistsAsync(string id)
        {
            return await _context.Employees.AnyAsync(e => e.EmployeeID == id);
        }
    }
}