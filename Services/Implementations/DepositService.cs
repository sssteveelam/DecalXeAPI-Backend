// DecalXeAPI/Services/Implementations/DepositService.cs
using AutoMapper;
using DecalXeAPI.Data;
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Implementations
{
    public class DepositService : IDepositService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<DepositService> _logger;

        public DepositService(ApplicationDbContext context, IMapper mapper, ILogger<DepositService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<DepositDto>> GetDepositsByOrderIdAsync(string orderId)
        {
            var deposits = await _context.Deposits
                                         .Where(d => d.OrderID == orderId)
                                         .ToListAsync();
            return _mapper.Map<List<DepositDto>>(deposits);
        }

        public async Task<DepositDto?> GetDepositByIdAsync(string depositId)
        {
            var deposit = await _context.Deposits.FindAsync(depositId);
            return _mapper.Map<DepositDto>(deposit);
        }

        public async Task<DepositDto> CreateDepositAsync(Deposit deposit)
        {
            // Kiểm tra xem OrderID có tồn tại không
            var orderExists = await _context.Orders.AnyAsync(o => o.OrderID == deposit.OrderID);
            if (!orderExists)
            {
                throw new ArgumentException($"Đơn hàng với ID '{deposit.OrderID}' không tồn tại.");
            }

            _context.Deposits.Add(deposit);
            await _context.SaveChangesAsync();

            // Sau khi lưu, có thể nạp lại thông tin nếu cần, nhưng ở đây không cần thiết

            return _mapper.Map<DepositDto>(deposit);
        }
    }
}