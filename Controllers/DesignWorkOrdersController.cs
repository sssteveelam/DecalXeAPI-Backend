// DecalXeAPI/Controllers/DesignWorkOrdersController.cs
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using DecalXeAPI.Data;
using AutoMapper;


namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,Sales,Designer")]
    public class DesignWorkOrdersController : ControllerBase
    {
        private readonly IDesignWorkOrderService _workOrderService;
        private readonly IMapper _mapper; // Thêm IMapper
        private readonly ApplicationDbContext _context;
        public DesignWorkOrdersController(IDesignWorkOrderService workOrderService, IMapper mapper, ApplicationDbContext context)
        {
            _workOrderService = workOrderService;
            _mapper = mapper;
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWorkOrder(string id)
        {
            var workOrder = await _workOrderService.GetWorkOrderByIdAsync(id);
            if (workOrder == null) return NotFound();
            return Ok(workOrder);
        }

        // API: POST /api/DesignWorkOrders (ĐÃ NÂNG CẤP)
        [HttpPost]
        public async Task<IActionResult> CreateWorkOrder(CreateDesignWorkOrderDto createDto)
        {
            var workOrder = _mapper.Map<DesignWorkOrder>(createDto);
            try
            {
                var createdWorkOrder = await _workOrderService.CreateWorkOrderAsync(workOrder);
                return CreatedAtAction(nameof(GetWorkOrder), new { id = createdWorkOrder.WorkOrderID }, createdWorkOrder);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // API: PUT /api/DesignWorkOrders/{id} (ĐÃ NÂNG CẤP)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWorkOrder(string id, UpdateDesignWorkOrderDto updateDto)
        {
            var workOrder = await _context.DesignWorkOrders.FindAsync(id);
            if (workOrder == null)
            {
                return NotFound();
            }

            _mapper.Map(updateDto, workOrder);

            try
            {
                var updatedWorkOrder = await _workOrderService.UpdateWorkOrderAsync(id, workOrder);
                if (updatedWorkOrder == null) return NotFound();
                return Ok(updatedWorkOrder);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}