// DecalXeAPI/Services/Implementations/DesignService.cs
using AutoMapper;
using DecalXeAPI.Data;
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Implementations
{
    public class DesignService : IDesignService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<DesignService> _logger;

        public DesignService(ApplicationDbContext context, IMapper mapper, ILogger<DesignService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<DesignDto>> GetDesignsAsync()
        {
            var designs = await _context.Designs.Include(d => d.Designer).ToListAsync();
            return _mapper.Map<List<DesignDto>>(designs);
        }

        public async Task<DesignDto?> GetDesignByIdAsync(string id)
        {
            var design = await _context.Designs
                                       .Include(d => d.Designer)
                                       .FirstOrDefaultAsync(d => d.DesignID == id);
            return design == null ? null : _mapper.Map<DesignDto>(design);
        }

        public async Task<DesignDto> CreateDesignAsync(Design design)
        {
            _logger.LogInformation("Tạo thiết kế mới.");
            // Bỏ logic kiểm tra OrderID
            if (!string.IsNullOrEmpty(design.DesignerID) && !await EmployeeExistsAsync(design.DesignerID))
            {
                throw new ArgumentException("DesignerID không tồn tại.");
            }

            _context.Designs.Add(design);
            await _context.SaveChangesAsync();

            await _context.Entry(design).Reference(d => d.Designer).LoadAsync();

            return _mapper.Map<DesignDto>(design);
        }

        public async Task<bool> UpdateDesignAsync(string id, Design design)
        {
            if (id != design.DesignID) return false;

            // Bỏ logic kiểm tra OrderID
            if (!string.IsNullOrEmpty(design.DesignerID) && !await EmployeeExistsAsync(design.DesignerID))
            {
                throw new ArgumentException("DesignerID không tồn tại.");
            }

            _context.Entry(design).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await DesignExistsAsync(id)) return false;
                else throw;
            }
            return true;
        }

        public async Task<bool> DeleteDesignAsync(string id)
        {
            var design = await _context.Designs.FindAsync(id);
            if (design == null) return false;
            _context.Designs.Remove(design);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DesignExistsAsync(string id) => await _context.Designs.AnyAsync(e => e.DesignID == id);
        public async Task<bool> EmployeeExistsAsync(string id) => await _context.Employees.AnyAsync(e => e.EmployeeID == id);
        // Bỏ hàm OrderExistsAsync vì không còn dùng đến
    }
}