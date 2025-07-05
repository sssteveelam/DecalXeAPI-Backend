// DecalXeAPI/Controllers/DepositsController.cs
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using AutoMapper; // <--- THÊM DÒNG NÀY VÀO


namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,Sales,Accountant")]
    public class DepositsController : ControllerBase
    {
          private readonly IDepositService _depositService;
        private readonly IMapper _mapper; // Thêm IMapper
        private readonly ILogger<DepositsController> _logger;

        // Tiêm IMapper vào constructor
        public DepositsController(IDepositService depositService, ILogger<DepositsController> logger, IMapper mapper)
        {
            _depositService = depositService;
            _logger = logger;
            _mapper = mapper; // Gán giá trị
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

        // API: POST api/Deposits (ĐÃ NÂNG CẤP)
        [HttpPost]
        public async Task<IActionResult> CreateDeposit(CreateDepositDto createDto)
        {
            // Dùng AutoMapper để "dịch" từ DTO sang Model
            var deposit = _mapper.Map<Deposit>(createDto);
            
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