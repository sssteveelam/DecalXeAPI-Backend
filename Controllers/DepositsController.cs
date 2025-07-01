// DecalXeAPI/Controllers/DepositsController.cs
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,Sales,Accountant")]
    public class DepositsController : ControllerBase
    {
        private readonly IDepositService _depositService;
        private readonly ILogger<DepositsController> _logger;

        public DepositsController(IDepositService depositService, ILogger<DepositsController> logger)
        {
            _depositService = depositService;
            _logger = logger;
        }

        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetDepositsByOrder(string orderId)
        {
            var deposits = await _depositService.GetDepositsByOrderIdAsync(orderId);
            return Ok(deposits);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDeposit(string id)
        {
            var deposit = await _depositService.GetDepositByIdAsync(id);
            if (deposit == null) return NotFound();
            return Ok(deposit);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDeposit(Deposit deposit)
        {
            try
            {
                var createdDeposit = await _depositService.CreateDepositAsync(deposit);
                return CreatedAtAction(nameof(GetDeposit), new { id = createdDeposit.DepositID }, createdDeposit);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}