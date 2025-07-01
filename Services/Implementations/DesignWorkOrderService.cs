// DecalXeAPI/Services/Implementations/DesignWorkOrderService.cs
using AutoMapper;
using DecalXeAPI.Data;
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Implementations
{
    public class DesignWorkOrderService : IDesignWorkOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<DesignWorkOrderService> _logger;

        public DesignWorkOrderService(ApplicationDbContext context, IMapper mapper, ILogger<DesignWorkOrderService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<DesignWorkOrderDto?> GetWorkOrderByIdAsync(string workOrderId)
        {
            var workOrder = await _context.DesignWorkOrders.FindAsync(workOrderId);
            return _mapper.Map<DesignWorkOrderDto>(workOrder);
        }

        public async Task<DesignWorkOrderDto?> GetWorkOrderByDesignIdAsync(string designId)
        {
            var workOrder = await _context.DesignWorkOrders
                                          .FirstOrDefaultAsync(wo => wo.DesignID == designId);
            return _mapper.Map<DesignWorkOrderDto>(workOrder);
        }

        public async Task<DesignWorkOrderDto> CreateWorkOrderAsync(DesignWorkOrder workOrder)
        {
            // Kiểm tra sự tồn tại của các khóa ngoại
            if (!await _context.Designs.AnyAsync(d => d.DesignID == workOrder.DesignID))
                throw new ArgumentException($"Thiết kế với ID '{workOrder.DesignID}' không tồn tại.");
            if (!await _context.Orders.AnyAsync(o => o.OrderID == workOrder.OrderID))
                throw new ArgumentException($"Đơn hàng với ID '{workOrder.OrderID}' không tồn tại.");

            _context.DesignWorkOrders.Add(workOrder);
            await _context.SaveChangesAsync();
            return _mapper.Map<DesignWorkOrderDto>(workOrder);
        }

        public async Task<DesignWorkOrderDto?> UpdateWorkOrderAsync(string workOrderId, DesignWorkOrder workOrder)
        {
            if (workOrderId != workOrder.WorkOrderID)
                throw new ArgumentException("ID không khớp.");

            var existingWorkOrder = await _context.DesignWorkOrders.FindAsync(workOrderId);
            if (existingWorkOrder == null) return null;

            // Cập nhật các thuộc tính
            existingWorkOrder.ActualHours = workOrder.ActualHours;
            existingWorkOrder.Cost = workOrder.Cost;
            existingWorkOrder.Status = workOrder.Status;
            existingWorkOrder.Requirements = workOrder.Requirements;

            await _context.SaveChangesAsync();
            return _mapper.Map<DesignWorkOrderDto>(existingWorkOrder);
        }
    }
}