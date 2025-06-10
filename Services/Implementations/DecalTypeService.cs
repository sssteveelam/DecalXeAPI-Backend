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
using System;

namespace DecalXeAPI.Services.Implementations
{
    public class DecalTypeService : IDecalTypeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<DecalTypeService> _logger;

        public DecalTypeService(ApplicationDbContext context, IMapper mapper, ILogger<DecalTypeService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<DecalTypeDto>> GetDecalTypesAsync()
        {
            _logger.LogInformation("Lấy danh sách loại decal.");
            var decalTypes = await _context.DecalTypes.ToListAsync();
            var decalTypeDtos = _mapper.Map<List<DecalTypeDto>>(decalTypes);
            return decalTypeDtos;
        }

        public async Task<DecalTypeDto?> GetDecalTypeByIdAsync(string id)
        {
            _logger.LogInformation("Yêu cầu lấy loại decal với ID: {DecalTypeID}", id);
            var decalType = await _context.DecalTypes.FindAsync(id);

            if (decalType == null)
            {
                _logger.LogWarning("Không tìm thấy loại decal với ID: {DecalTypeID}", id);
                return null;
            }

            var decalTypeDto = _mapper.Map<DecalTypeDto>(decalType);
            _logger.LogInformation("Đã trả về loại decal với ID: {DecalTypeID}", id);
            return decalTypeDto;
        }

        public async Task<DecalTypeDto> CreateDecalTypeAsync(DecalType decalType)
        {
            _logger.LogInformation("Yêu cầu tạo loại decal mới: {DecalTypeName}", decalType.DecalTypeName);
            _context.DecalTypes.Add(decalType);
            await _context.SaveChangesAsync();

            var decalTypeDto = _mapper.Map<DecalTypeDto>(decalType);
            _logger.LogInformation("Đã tạo loại decal mới với ID: {DecalTypeID}", decalType.DecalTypeID);
            return decalTypeDto;
        }

        public async Task<bool> UpdateDecalTypeAsync(string id, DecalType decalType)
        {
            _logger.LogInformation("Yêu cầu cập nhật loại decal với ID: {DecalTypeID}", id);

            if (id != decalType.DecalTypeID)
            {
                _logger.LogWarning("ID trong tham số ({Id}) không khớp với DecalTypeID trong body ({DecalTypeIDBody})", id, decalType.DecalTypeID);
                return false;
            }

            if (!await DecalTypeExistsAsync(id))
            {
                _logger.LogWarning("Không tìm thấy loại decal để cập nhật với ID: {DecalTypeID}", id);
                return false;
            }

            _context.Entry(decalType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã cập nhật loại decal với ID: {DecalTypeID}", id);
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Lỗi xung đột khi cập nhật loại decal với ID: {DecalTypeID}", id);
                throw;
            }
        }

        public async Task<bool> DeleteDecalTypeAsync(string id)
        {
            _logger.LogInformation("Yêu cầu xóa loại decal với ID: {DecalTypeID}", id);
            var decalType = await _context.DecalTypes.FindAsync(id);
            if (decalType == null)
            {
                _logger.LogWarning("Không tìm thấy loại decal để xóa với ID: {DecalTypeID}", id);
                return false;
            }

            _context.DecalTypes.Remove(decalType);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Đã xóa loại decal với ID: {DecalTypeID}", id);
            return true;
        }

        public async Task<bool> DecalTypeExistsAsync(string id)
        {
            return await _context.DecalTypes.AnyAsync(e => e.DecalTypeID == id);
        }
    }
}