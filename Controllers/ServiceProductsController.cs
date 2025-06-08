using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs; // Để sử dụng ServiceProductDto
using AutoMapper; // Để sử dụng AutoMapper
using System.Collections.Generic; // Để sử dụng IEnumerable

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper; // Khai báo biến IMapper

        public ServiceProductsController(ApplicationDbContext context, IMapper mapper) // Tiêm IMapper
        {
            _context = context;
            _mapper = mapper;
        }

        // API: GET api/ServiceProducts
        // Lấy tất cả các ServiceProduct, bao gồm thông tin DecalService và Product liên quan, trả về DTO
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceProductDto>>> GetServiceProducts() // Kiểu trả về là ServiceProductDto
        {
            var serviceProducts = await _context.ServiceProducts
                                                .Include(sp => sp.DecalService)
                                                .Include(sp => sp.Product)
                                                .ToListAsync();
            // Sử dụng AutoMapper để ánh xạ từ List<ServiceProduct> sang List<ServiceProductDto>
            var serviceProductDtos = _mapper.Map<List<ServiceProductDto>>(serviceProducts);
            return Ok(serviceProductDtos);
        }

        // API: GET api/ServiceProducts/{id}
        // Lấy thông tin một ServiceProduct theo ServiceProductID, bao gồm các thông tin liên quan, trả về DTO
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceProductDto>> GetServiceProduct(string id) // Kiểu trả về là ServiceProductDto
        {
            var serviceProduct = await _context.ServiceProducts
                                                    .Include(sp => sp.DecalService)
                                                    .Include(sp => sp.Product)
                                                    .FirstOrDefaultAsync(sp => sp.ServiceProductID == id);

            if (serviceProduct == null)
            {
                return NotFound();
            }

            // Sử dụng AutoMapper để ánh xạ từ ServiceProduct Model sang ServiceProductDto
            var serviceProductDto = _mapper.Map<ServiceProductDto>(serviceProduct);
            return Ok(serviceProductDto);
        }

        // API: POST api/ServiceProducts
        // Tạo một ServiceProduct mới, nhận vào ServiceProduct Model, trả về ServiceProductDto sau khi tạo
        [HttpPost]
        public async Task<ActionResult<ServiceProductDto>> PostServiceProduct(ServiceProduct serviceProduct) // Kiểu trả về là ServiceProductDto
        {
            // Kiểm tra xem ServiceID và ProductID có tồn tại không
            if (!string.IsNullOrEmpty(serviceProduct.ServiceID) && !DecalServiceExists(serviceProduct.ServiceID))
            {
                return BadRequest("ServiceID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(serviceProduct.ProductID) && !ProductExists(serviceProduct.ProductID))
            {
                return BadRequest("ProductID không tồn tại.");
            }

            _context.ServiceProducts.Add(serviceProduct);
            await _context.SaveChangesAsync();

            // Tải lại thông tin liên quan để AutoMapper có thể ánh xạ đầy đủ
            await _context.Entry(serviceProduct).Reference(sp => sp.DecalService).LoadAsync();
            await _context.Entry(serviceProduct).Reference(sp => sp.Product).LoadAsync();

            // Ánh xạ ServiceProduct Model vừa tạo sang ServiceProductDto để trả về
            var serviceProductDto = _mapper.Map<ServiceProductDto>(serviceProduct);
            return CreatedAtAction(nameof(GetServiceProduct), new { id = serviceProductDto.ServiceProductID }, serviceProductDto);
        }

        // API: PUT api/ServiceProducts/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutServiceProduct(string id, ServiceProduct serviceProduct)
        {
            if (id != serviceProduct.ServiceProductID)
            {
                return BadRequest();
            }

            if (!string.IsNullOrEmpty(serviceProduct.ServiceID) && !DecalServiceExists(serviceProduct.ServiceID))
            {
                return BadRequest("ServiceID không tồn tại.");
            }
            if (!string.IsNullOrEmpty(serviceProduct.ProductID) && !ProductExists(serviceProduct.ProductID))
            {
                return BadRequest("ProductID không tồn tại.");
            }

            _context.Entry(serviceProduct).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceProductExists(id))
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

        // API: DELETE api/ServiceProducts/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceProduct(string id)
        {
            var serviceProduct = await _context.ServiceProducts.FindAsync(id);
            if (serviceProduct == null)
            {
                return NotFound();
            }

            _context.ServiceProducts.Remove(serviceProduct);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ServiceProductExists(string id)
        {
            return _context.ServiceProducts.Any(e => e.ServiceProductID == id);
        }

        private bool DecalServiceExists(string id)
        {
            return _context.DecalServices.Any(e => e.ServiceID == id);
        }

        private bool ProductExists(string id)
        {
            return _context.Products.Any(e => e.ProductID == id);
        }
    }
}