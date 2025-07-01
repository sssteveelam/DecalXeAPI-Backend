// DecalXeAPI/Controllers/DesignWorkOrdersController.cs
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,Sales,Designer")]
    public class DesignWorkOrdersController : ControllerBase
    {
        private readonly IDesignWorkOrderService _workOrderService;

        public DesignWorkOrdersController(IDesignWorkOrderService workOrderService)
        {
            _workOrderService = workOrderService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWorkOrder(string id)
        {
            var workOrder = await _workOrderService.GetWorkOrderByIdAsync(id);
            if (workOrder == null) return NotFound();
            return Ok(workOrder);
        }

        [HttpPost]
        public async Task<IActionResult> CreateWorkOrder(DesignWorkOrder workOrder)
        {
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWorkOrder(string id, DesignWorkOrder workOrder)
        {
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