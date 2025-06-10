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

namespace DecalXeAPI.Services.Implementations
{
    public class ScheduledWorkUnitService : IScheduledWorkUnitService // <-- Kế thừa từ IScheduledWorkUnitService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ScheduledWorkUnitService> _logger;

        public ScheduledWorkUnitService(ApplicationDbContext context, IMapper mapper, ILogger<ScheduledWorkUnitService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // Lấy danh sách ScheduledWorkUnit
        public async Task<IEnumerable<ScheduledWorkUnitDto>> GetScheduledWorkUnitsAsync()
        {
            _logger.LogInformation("Lấy danh sách các đơn vị công việc đã lên lịch.");
            var scheduledWorkUnits = await _context.ScheduledWorkUnits
                                                    .Include(swu => swu.DailySchedule)
                                                    .Include(swu => swu.TimeSlotDefinition)
                                                    .Include(swu => swu.Order)
                                                    .ToListAsync();
            var scheduledWorkUnitDtos = _mapper.Map<List<ScheduledWorkUnitDto>>(scheduledWorkUnits);
            return scheduledWorkUnitDtos;
        }

        // Lấy ScheduledWorkUnit theo ID
        public async Task<ScheduledWorkUnitDto?> GetScheduledWorkUnitByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy đơn vị công việc đã lên lịch với ID: {ScheduledWorkUnitID}", id);
            var scheduledWorkUnit = await _context.ScheduledWorkUnits
                                                    .Include(swu => swu.DailySchedule)
                                                    .Include(swu => swu.TimeSlotDefinition)
                                                    .Include(swu => swu.Order)
                                                    .FirstOrDefaultAsync(swu => swu.ScheduledWorkUnitID == id);

            if (scheduledWorkUnit == null)
            {
                _logger.LogWarning("Không tìm thấy đơn vị công việc đã lên lịch với ID: {ScheduledWorkUnitID}", id);
                return null;
            }

            var scheduledWorkUnitDto = _mapper.Map<ScheduledWorkUnitDto>(scheduledWorkUnit);
            _logger.LogInformation("Đã trả về đơn vị công việc đã lên lịch với ID: {ScheduledWorkUnitID}", id);
            return scheduledWorkUnitDto;
        }

        // Tạo ScheduledWorkUnit mới
        public async Task<(ScheduledWorkUnitDto? ScheduledWorkUnit, string? ErrorMessage)> CreateScheduledWorkUnitAsync(ScheduledWorkUnit scheduledWorkUnit)
        {
            _logger.LogInformation("Yêu cầu tạo đơn vị công việc đã lên lịch mới cho DailyScheduleID: {DailyScheduleID}, SlotDefID: {SlotDefID}",
                                    scheduledWorkUnit.DailyScheduleID, scheduledWorkUnit.SlotDefID);

            // Kiểm tra FKs
            if (!await TechnicianDailyScheduleExistsAsync(scheduledWorkUnit.DailyScheduleID))
            {
                return (null, "DailyScheduleID không tồn tại.");
            }
            if (!await TimeSlotDefinitionExistsAsync(scheduledWorkUnit.SlotDefID))
            {
                return (null, "SlotDefID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(scheduledWorkUnit.OrderID) && !await OrderExistsAsync(scheduledWorkUnit.OrderID))
            {
                return (null, "OrderID không tồn tại.");
            }

            // Kiểm tra xem slot này đã bị gán cho Order khác hoặc đã bị Booked chưa
            var existingWorkUnit = await _context.ScheduledWorkUnits
                                                .FirstOrDefaultAsync(swu =>
                                                    swu.DailyScheduleID == scheduledWorkUnit.DailyScheduleID &&
                                                    swu.SlotDefID == scheduledWorkUnit.SlotDefID); // Không loại trừ chính nó khi tạo mới
            if (existingWorkUnit != null && existingWorkUnit.OrderID != null)
            {
                return (null, "Khung giờ này đã được đặt.");
            }

            // Nếu muốn gán cho Order, OrderID không được null
            if (scheduledWorkUnit.Status == "Booked" && string.IsNullOrEmpty(scheduledWorkUnit.OrderID))
            {
                return (null, "OrderID không thể rỗng khi trạng thái là 'Booked'.");
            }
            if (scheduledWorkUnit.Status == "Available" && !string.IsNullOrEmpty(scheduledWorkUnit.OrderID))
            {
                return (null, "OrderID phải rỗng khi trạng thái là 'Available'.");
            }

            _context.ScheduledWorkUnits.Add(scheduledWorkUnit);
            await _context.SaveChangesAsync();

            // Cập nhật trạng thái Order (nếu có)
            if (!string.IsNullOrEmpty(scheduledWorkUnit.OrderID))
            {
                await UpdateOrderStatusBasedOnScheduledWorkUnits(scheduledWorkUnit.OrderID);
            }

            // Tải lại thông tin liên quan để AutoMapper có thể ánh xạ đầy đủ
            await _context.Entry(scheduledWorkUnit).Reference(swu => swu.DailySchedule).LoadAsync();
            await _context.Entry(scheduledWorkUnit).Reference(swu => swu.TimeSlotDefinition).LoadAsync();
            await _context.Entry(scheduledWorkUnit).Reference(swu => swu.Order).LoadAsync();

            var scheduledWorkUnitDto = _mapper.Map<ScheduledWorkUnitDto>(scheduledWorkUnit);
            _logger.LogInformation("Đã tạo đơn vị công việc đã lên lịch mới với ID: {ScheduledWorkUnitID}", scheduledWorkUnit.ScheduledWorkUnitID);
            return (scheduledWorkUnitDto, null);
        }

        // Cập nhật ScheduledWorkUnit
        public async Task<(bool Success, string? ErrorMessage)> UpdateScheduledWorkUnitAsync(string id, ScheduledWorkUnit scheduledWorkUnit)
        {
            _logger.LogInformation("Yêu cầu cập nhật đơn vị công việc đã lên lịch với ID: {ScheduledWorkUnitID}", id);

            if (id != scheduledWorkUnit.ScheduledWorkUnitID)
            {
                _logger.LogWarning("ID trong tham số ({Id}) không khớp với ScheduledWorkUnitID trong body ({ScheduledWorkUnitIDBody})", id, scheduledWorkUnit.ScheduledWorkUnitID);
                return (false, "ID không khớp.");
            }

            var oldScheduledWorkUnit = await _context.ScheduledWorkUnits.AsNoTracking().FirstOrDefaultAsync(swu => swu.ScheduledWorkUnitID == id);
            if (oldScheduledWorkUnit == null)
            {
                _logger.LogWarning("Không tìm thấy đơn vị công việc đã lên lịch cũ để cập nhật với ID: {ScheduledWorkUnitID}", id);
                return (false, "Đơn vị công việc đã lên lịch không tồn tại.");
            }

            // Kiểm tra FKs
            if (!await TechnicianDailyScheduleExistsAsync(scheduledWorkUnit.DailyScheduleID))
            {
                return (false, "DailyScheduleID không tồn tại.");
            }
            if (!await TimeSlotDefinitionExistsAsync(scheduledWorkUnit.SlotDefID))
            {
                return (false, "SlotDefID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(scheduledWorkUnit.OrderID) && !await OrderExistsAsync(scheduledWorkUnit.OrderID))
            {
                return (false, "OrderID không tồn tại.");
            }

            // Kiểm tra trùng lịch hoặc gán đơn hàng (khi cập nhật)
            var existingWorkUnitConflict = await _context.ScheduledWorkUnits
                                                            .FirstOrDefaultAsync(swu =>
                                                                swu.DailyScheduleID == scheduledWorkUnit.DailyScheduleID &&
                                                                swu.SlotDefID == scheduledWorkUnit.SlotDefID &&
                                                                swu.ScheduledWorkUnitID != id); // Loại trừ chính nó
            if (existingWorkUnitConflict != null && existingWorkUnitConflict.OrderID != null)
            {
                return (false, "Khung giờ này đã được đặt bởi Order khác.");
            }

            // Ràng buộc trạng thái OrderID và Status
            if (scheduledWorkUnit.Status == "Booked" && string.IsNullOrEmpty(scheduledWorkUnit.OrderID))
            {
                return (false, "OrderID không thể rỗng khi trạng thái là 'Booked'.");
            }
            if (scheduledWorkUnit.Status == "Available" && !string.IsNullOrEmpty(scheduledWorkUnit.OrderID))
            {
                 return (false, "OrderID phải rỗng khi trạng thái là 'Available'.");
            }

            _context.Entry(scheduledWorkUnit).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                // Cập nhật trạng thái Order cũ (nếu OrderID thay đổi)
                if (oldScheduledWorkUnit.OrderID != null && oldScheduledWorkUnit.OrderID != scheduledWorkUnit.OrderID)
                {
                    await UpdateOrderStatusBasedOnScheduledWorkUnits(oldScheduledWorkUnit.OrderID);
                }
                // Cập nhật trạng thái Order mới (nếu OrderID được gán/thay đổi)
                if (!string.IsNullOrEmpty(scheduledWorkUnit.OrderID))
                {
                    await UpdateOrderStatusBasedOnScheduledWorkUnits(scheduledWorkUnit.OrderID);
                }

                _logger.LogInformation("Đã cập nhật đơn vị công việc đã lên lịch với ID: {ScheduledWorkUnitID}", id);
                return (true, null);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật đơn vị công việc đã lên lịch với ID: {ScheduledWorkUnitID}", id);
                throw;
            }
        }

        // Xóa ScheduledWorkUnit
        public async Task<(bool Success, string? ErrorMessage)> DeleteScheduledWorkUnitAsync(string id)
        {
            _logger.LogInformation("Yêu cầu xóa đơn vị công việc đã lên lịch với ID: {ScheduledWorkUnitID}", id);

            var scheduledWorkUnit = await _context.ScheduledWorkUnits.FindAsync(id);
            if (scheduledWorkUnit == null)
            {
                _logger.LogWarning("Không tìm thấy đơn vị công việc đã lên lịch để xóa với ID: {ScheduledWorkUnitID}", id);
                return (false, "Đơn vị công việc đã lên lịch không tồn tại.");
            }

            _context.ScheduledWorkUnits.Remove(scheduledWorkUnit);
            await _context.SaveChangesAsync();

            // Cập nhật trạng thái Order (nếu có)
            if (!string.IsNullOrEmpty(scheduledWorkUnit.OrderID))
            {
                await UpdateOrderStatusBasedOnScheduledWorkUnits(scheduledWorkUnit.OrderID);
            }

            _logger.LogInformation("Đã xóa đơn vị công việc đã lên lịch với ID: {ScheduledWorkUnitID}", id);
            return (true, null);
        }

        // Hàm hỗ trợ: Cập nhật trạng thái Order dựa trên các ScheduledWorkUnit
        private async Task UpdateOrderStatusBasedOnScheduledWorkUnits(string orderId)
        {
            var order = await _context.Orders
                                    .Include(o => o.ScheduledWorkUnits)
                                    .Include(o => o.OrderDetails) // Cần OrderDetails để lấy StandardWorkUnits
                                        .ThenInclude(od => od.DecalService) // Cần DecalService để lấy StandardWorkUnits
                                    .FirstOrDefaultAsync(o => o.OrderID == orderId);

            if (order == null)
            {
                _logger.LogWarning("Không tìm thấy Order {OrderID} khi cập nhật trạng thái từ ScheduledWorkUnit.", orderId);
                return;
            }

            var totalWorkUnitsForOrder = await _context.ScheduledWorkUnits
                                                        .Where(swu => swu.OrderID == orderId)
                                                        .ToListAsync();

            var completedWorkUnits = totalWorkUnitsForOrder.Count(swu => swu.Status == "Completed");
            var bookedWorkUnits = totalWorkUnitsForOrder.Count(swu => swu.Status == "Booked");

            // Tính tổng số StandardWorkUnits cần thiết cho Order này từ OrderDetails
            decimal totalRequiredWorkUnits = 0;
            if (order.OrderDetails != null)
            {
                foreach(var od in order.OrderDetails)
                {
                    var decalService = await _context.DecalServices.FindAsync(od.ServiceID);
                    if (decalService != null)
                    {
                        totalRequiredWorkUnits += od.Quantity * decalService.StandardWorkUnits;
                    }
                }
            }

            string newOrderStatus;

            if (totalRequiredWorkUnits == 0)
            {
                newOrderStatus = "Ready For Payment"; // Nếu không có WorkUnit nào cần (ví dụ: chỉ tư vấn), coi là sẵn sàng thanh toán
            }
            else if (completedWorkUnits == totalRequiredWorkUnits)
            {
                newOrderStatus = "Completed";
            }
            else if (bookedWorkUnits > 0 || completedWorkUnits > 0)
            {
                newOrderStatus = "In Progress";
            }
            else if (totalWorkUnitsForOrder.Any(swu => swu.Status == "Available")) // Có WorkUnit đã lên lịch nhưng chưa gán hoặc chưa bắt đầu
            {
                newOrderStatus = "Assigned";
            }
            else
            {
                newOrderStatus = "New"; // Mặc định nếu không có WorkUnit nào được gán hoặc đang xử lý
            }

            if (order.OrderStatus != newOrderStatus) // Chỉ cập nhật nếu trạng thái thay đổi
            {
                order.OrderStatus = newOrderStatus;
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Order {OrderID} đã tự động chuyển trạng thái từ '{OldStatus}' sang '{NewStatus}'", order.OrderID, order.OrderStatus, newOrderStatus);
            }
        }

        // --- HÀM HỖ TRỢ: KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG (PUBLIC CHO INTERFACE) ---
        public async Task<bool> ScheduledWorkUnitExistsAsync(string id)
        {
            return await _context.ScheduledWorkUnits.AnyAsync(e => e.ScheduledWorkUnitID == id);
        }

        public async Task<bool> TechnicianDailyScheduleExistsAsync(string id)
        {
            return await _context.TechnicianDailySchedules.AnyAsync(e => e.DailyScheduleID == id);
        }

        public async Task<bool> TimeSlotDefinitionExistsAsync(string id)
        {
            return await _context.TimeSlotDefinitions.AnyAsync(e => e.SlotDefID == id);
        }

        public async Task<bool> OrderExistsAsync(string id)
        {
            return await _context.Orders.AnyAsync(e => e.OrderID == id);
        }
    }
}