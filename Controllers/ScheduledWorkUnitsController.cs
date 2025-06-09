using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs;
using AutoMapper;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,Technician")]
    public class ScheduledWorkUnitsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ScheduledWorkUnitsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // API: GET api/ScheduledWorkUnits
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ScheduledWorkUnitDto>>> GetScheduledWorkUnits()
        {
            var scheduledWorkUnits = await _context.ScheduledWorkUnits
                                                    .Include(swu => swu.DailySchedule)
                                                    .Include(swu => swu.TimeSlotDefinition)
                                                    .Include(swu => swu.Order)
                                                    .ToListAsync();
            var scheduledWorkUnitDtos = _mapper.Map<List<ScheduledWorkUnitDto>>(scheduledWorkUnits);
            return Ok(scheduledWorkUnitDtos);
        }

        // API: GET api/ScheduledWorkUnits/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ScheduledWorkUnitDto>> GetScheduledWorkUnit(string id)
        {
            var scheduledWorkUnit = await _context.ScheduledWorkUnits
                                                    .Include(swu => swu.DailySchedule)
                                                    .Include(swu => swu.TimeSlotDefinition)
                                                    .Include(swu => swu.Order)
                                                    .FirstOrDefaultAsync(swu => swu.ScheduledWorkUnitID == id);

            if (scheduledWorkUnit == null)
            {
                return NotFound();
            }

            var scheduledWorkUnitDto = _mapper.Map<ScheduledWorkUnitDto>(scheduledWorkUnit);
            return Ok(scheduledWorkUnitDto);
        }

        // API: POST api/ScheduledWorkUnits
        // Thêm logic cập nhật trạng thái Order và kiểm tra ràng buộc lịch trình
        [HttpPost]
        public async Task<ActionResult<ScheduledWorkUnitDto>> PostScheduledWorkUnit(ScheduledWorkUnit scheduledWorkUnit)
        {
            // Kiểm tra FKs
            if (!string.IsNullOrEmpty(scheduledWorkUnit.DailyScheduleID) && !TechnicianDailyScheduleExists(scheduledWorkUnit.DailyScheduleID))
            {
                return BadRequest("DailyScheduleID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(scheduledWorkUnit.SlotDefID) && !TimeSlotDefinitionExists(scheduledWorkUnit.SlotDefID))
            {
                return BadRequest("SlotDefID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(scheduledWorkUnit.OrderID) && !OrderExists(scheduledWorkUnit.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }

            // --- LOGIC NGHIỆP VỤ: KIỂM TRA TRÙNG LỊCH HOẶC GÁN ĐƠN HÀNG ---
            // Kiểm tra xem slot này đã bị gán cho Order khác hoặc đã bị Booked chưa
            var existingWorkUnit = await _context.ScheduledWorkUnits
                                                .FirstOrDefaultAsync(swu =>
                                                    swu.DailyScheduleID == scheduledWorkUnit.DailyScheduleID &&
                                                    swu.SlotDefID == scheduledWorkUnit.SlotDefID &&
                                                    swu.ScheduledWorkUnitID != scheduledWorkUnit.ScheduledWorkUnitID); // Loại trừ chính nó khi cập nhật
            if (existingWorkUnit != null && existingWorkUnit.OrderID != null)
            {
                return BadRequest("Khung giờ này đã được đặt.");
            }

            // Nếu muốn gán cho Order, OrderID không được null
            if (scheduledWorkUnit.Status == "Booked" && string.IsNullOrEmpty(scheduledWorkUnit.OrderID))
            {
                return BadRequest("OrderID không thể rỗng khi trạng thái là 'Booked'.");
            }
            if (scheduledWorkUnit.Status == "Available" && !string.IsNullOrEmpty(scheduledWorkUnit.OrderID))
            {
                return BadRequest("OrderID phải rỗng khi trạng thái là 'Available'.");
            }

            _context.ScheduledWorkUnits.Add(scheduledWorkUnit);
            await _context.SaveChangesAsync();

            // --- LOGIC NGHIỆP VỤ: CẬP NHẬT TRẠNG THÁI ORDER (NẾU CÓ) ---
            if (!string.IsNullOrEmpty(scheduledWorkUnit.OrderID))
            {
                await UpdateOrderStatusBasedOnScheduledWorkUnits(scheduledWorkUnit.OrderID);
            }

            // Tải lại thông tin liên quan để AutoMapper có thể ánh xạ đầy đủ
            await _context.Entry(scheduledWorkUnit).Reference(swu => swu.DailySchedule).LoadAsync();
            await _context.Entry(scheduledWorkUnit).Reference(swu => swu.TimeSlotDefinition).LoadAsync();
            await _context.Entry(scheduledWorkUnit).Reference(swu => swu.Order).LoadAsync();

            var scheduledWorkUnitDto = _mapper.Map<ScheduledWorkUnitDto>(scheduledWorkUnit);
            return CreatedAtAction(nameof(GetScheduledWorkUnit), new { id = scheduledWorkUnitDto.ScheduledWorkUnitID }, scheduledWorkUnitDto);
        }

        // API: PUT api/ScheduledWorkUnits/{id}
        // Thêm logic cập nhật trạng thái Order và kiểm tra ràng buộc lịch trình
        [HttpPut("{id}")]
        public async Task<IActionResult> PutScheduledWorkUnit(string id, ScheduledWorkUnit scheduledWorkUnit)
        {
            if (id != scheduledWorkUnit.ScheduledWorkUnitID)
            {
                return BadRequest();
            }

            // Lấy ScheduledWorkUnit cũ để so sánh
            var oldScheduledWorkUnit = await _context.ScheduledWorkUnits.AsNoTracking().FirstOrDefaultAsync(swu => swu.ScheduledWorkUnitID == id);
            if (oldScheduledWorkUnit == null)
            {
                return NotFound();
            }

            // Kiểm tra FKs
            if (!string.IsNullOrEmpty(scheduledWorkUnit.DailyScheduleID) && !TechnicianDailyScheduleExists(scheduledWorkUnit.DailyScheduleID))
            {
                return BadRequest("DailyScheduleID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(scheduledWorkUnit.SlotDefID) && !TimeSlotDefinitionExists(scheduledWorkUnit.SlotDefID))
            {
                return BadRequest("SlotDefID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(scheduledWorkUnit.OrderID) && !OrderExists(scheduledWorkUnit.OrderID))
            {
                return BadRequest("OrderID không tồn tại.");
            }

            // --- LOGIC NGHIỆP VỤ: KIỂM TRA TRÙNG LỊCH HOẶC GÁN ĐƠN HÀNG (KHI CẬP NHẬT) ---
            var existingWorkUnitConflict = await _context.ScheduledWorkUnits
                                                            .FirstOrDefaultAsync(swu =>
                                                                swu.DailyScheduleID == scheduledWorkUnit.DailyScheduleID &&
                                                                swu.SlotDefID == scheduledWorkUnit.SlotDefID &&
                                                                swu.ScheduledWorkUnitID != id); // Loại trừ chính nó
            if (existingWorkUnitConflict != null && existingWorkUnitConflict.OrderID != null)
            {
                return BadRequest("Khung giờ này đã được đặt bởi Order khác.");
            }

            // Ràng buộc trạng thái OrderID và Status
            if (scheduledWorkUnit.Status == "Booked" && string.IsNullOrEmpty(scheduledWorkUnit.OrderID))
            {
                return BadRequest("OrderID không thể rỗng khi trạng thái là 'Booked'.");
            }
            if (scheduledWorkUnit.Status == "Available" && !string.IsNullOrEmpty(scheduledWorkUnit.OrderID))
            {
                return BadRequest("OrderID phải rỗng khi trạng thái là 'Available'.");
            }


            _context.Entry(scheduledWorkUnit).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                // --- LOGIC NGHIỆP VỤ: CẬP NHẬT TRẠNG THÁI ORDER (NẾU CÓ) ---
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

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ScheduledWorkUnitExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // API: DELETE api/ScheduledWorkUnits/{id}
        // Thêm logic cập nhật trạng thái Order sau khi xóa
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteScheduledWorkUnit(string id)
        {
            var scheduledWorkUnit = await _context.ScheduledWorkUnits.FindAsync(id);
            if (scheduledWorkUnit == null)
            {
                return NotFound();
            }

            _context.ScheduledWorkUnits.Remove(scheduledWorkUnit);
            await _context.SaveChangesAsync();

            // --- LOGIC NGHIỆP VỤ: CẬP NHẬT TRẠNG THÁI ORDER (NẾU CÓ) ---
            // Cập nhật trạng thái Order liên quan sau khi xóa ScheduledWorkUnit
            if (!string.IsNullOrEmpty(scheduledWorkUnit.OrderID))
            {
                await UpdateOrderStatusBasedOnScheduledWorkUnits(scheduledWorkUnit.OrderID);
            }

            return NoContent();
        }

        // --- HÀM HỖ TRỢ: CẬP NHẬT TRẠNG THÁI ORDER DỰA TRÊN CÁC SCHEDULEDWORKUNIT ---
        private async Task UpdateOrderStatusBasedOnScheduledWorkUnits(string orderId)
        {
            var order = await _context.Orders
                                    .Include(o => o.ScheduledWorkUnits) // Bao gồm các ScheduledWorkUnits của Order
                                    .FirstOrDefaultAsync(o => o.OrderID == orderId);

            if (order == null) return; // Không tìm thấy Order

            var totalWorkUnitsForOrder = await _context.ScheduledWorkUnits
                                                        .Where(swu => swu.OrderID == orderId)
                                                        .ToListAsync();

            var completedWorkUnits = totalWorkUnitsForOrder.Count(swu => swu.Status == "Completed");
            var bookedWorkUnits = totalWorkUnitsForOrder.Count(swu => swu.Status == "Booked");
            var totalRequiredWorkUnits = order.OrderDetails.Sum(od => od.Quantity * _context.DecalServices.Find(od.ServiceID).StandardWorkUnits); // Giả định StandardWorkUnits được định nghĩa trong DecalService

            // Logic cập nhật trạng thái Order:
            // Có thể cần chi tiết hơn tùy thuộc vào quy tắc nghiệp vụ thực tế
            if (totalRequiredWorkUnits == 0) // Nếu đơn hàng không yêu cầu WorkUnit (ví dụ: chỉ tư vấn)
            {
                if (order.OrderStatus == "New" || order.OrderStatus == "Pending")
                {
                    order.OrderStatus = "Ready For Payment"; // Sẵn sàng thanh toán
                }
            }
            else if (completedWorkUnits == totalRequiredWorkUnits) // Tất cả WorkUnit đã hoàn thành
            {
                order.OrderStatus = "Completed";
            }
            else if (bookedWorkUnits > 0 || completedWorkUnits > 0) // Có ít nhất một WorkUnit đã Booked hoặc Completed
            {
                order.OrderStatus = "In Progress";
            }
            else if (totalWorkUnitsForOrder.Any(swu => swu.Status == "Booked" || swu.Status == "Available")) // Có WorkUnit đã được gán hoặc sẵn sàng
            {
                order.OrderStatus = "Assigned"; // Đã được phân công lịch
            }
            else // Mặc định là New nếu không có WorkUnit nào được gán
            {
                order.OrderStatus = "New"; // Hoặc Pending
            }

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }


        // --- HÀM HỖ TRỢ: KIỂM TRA SỰ TỒN TẠI CỦA CÁC ĐỐI TƯỢNG ---
        private bool ScheduledWorkUnitExists(string id)
        {
            return _context.ScheduledWorkUnits.Any(e => e.ScheduledWorkUnitID == id);
        }

        private bool TechnicianDailyScheduleExists(string id)
        {
            return _context.TechnicianDailySchedules.Any(e => e.DailyScheduleID == id);
        }

        private bool TimeSlotDefinitionExists(string id)
        {
            return _context.TimeSlotDefinitions.Any(e => e.SlotDefID == id);
        }

        private bool OrderExists(string id)
        {
            return _context.Orders.Any(e => e.OrderID == id);
        }
    }
}