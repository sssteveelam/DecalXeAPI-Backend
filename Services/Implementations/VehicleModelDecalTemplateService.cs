// DecalXeAPI/Services/Implementations/VehicleModelDecalTemplateService.cs
using AutoMapper;
using DecalXeAPI.Data;
using DecalXeAPI.DTOs;
using DecalXeAPI.Models;
using DecalXeAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecalXeAPI.Services.Implementations
{
    public class VehicleModelDecalTemplateService : IVehicleModelDecalTemplateService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<VehicleModelDecalTemplateService> _logger;

        public VehicleModelDecalTemplateService(ApplicationDbContext context, IMapper mapper, ILogger<VehicleModelDecalTemplateService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<VehicleModelDecalTemplateDto>> GetAllAsync()
        {
            var links = await _context.VehicleModelDecalTemplates
                                      .Include(l => l.VehicleModel)
                                      .Include(l => l.DecalTemplate)
                                      .ToListAsync();
            return _mapper.Map<List<VehicleModelDecalTemplateDto>>(links);
        }

        public async Task<(VehicleModelDecalTemplateDto?, string?)> CreateAsync(VehicleModelDecalTemplate link)
        {
            if (!await _context.VehicleModels.AnyAsync(m => m.ModelID == link.ModelID))
                return (null, $"Mẫu xe với ID '{link.ModelID}' không tồn tại.");
            if (!await _context.DecalTemplates.AnyAsync(t => t.TemplateID == link.TemplateID))
                return (null, $"Mẫu decal với ID '{link.TemplateID}' không tồn tại.");

            _context.VehicleModelDecalTemplates.Add(link);
            await _context.SaveChangesAsync();

            await _context.Entry(link).Reference(l => l.VehicleModel).LoadAsync();
            await _context.Entry(link).Reference(l => l.DecalTemplate).LoadAsync();

            return (_mapper.Map<VehicleModelDecalTemplateDto>(link), null);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var link = await _context.VehicleModelDecalTemplates.FindAsync(id);
            if (link == null) return false;

            _context.VehicleModelDecalTemplates.Remove(link);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}