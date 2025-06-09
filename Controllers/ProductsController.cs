using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;
using DecalXeAPI.DTOs; // Để sử dụng ProductDto
using AutoMapper; // Để sử dụng AutoMapper
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization; // Để sử dụng IEnumerable

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,Inventory")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper; // Khai báo biến IMapper

        public ProductsController(ApplicationDbContext context, IMapper mapper) // Tiêm IMapper
        {
            _context = context;
            _mapper = mapper;
        }

        // API: GET api/Products
        // Lấy tất cả các Product, trả về dưới dạng ProductDto
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts() // Kiểu trả về là ProductDto
        {
            var products = await _context.Products.ToListAsync();
            // Sử dụng AutoMapper để ánh xạ từ List<Product> sang List<ProductDto>
            var productDtos = _mapper.Map<List<ProductDto>>(products);
            return Ok(productDtos);
        }

        // API: GET api/Products/{id}
        // Lấy thông tin một Product theo ProductID, trả về dưới dạng ProductDto
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(string id) // Kiểu trả về là ProductDto
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            // Sử dụng AutoMapper để ánh xạ từ Product Model sang ProductDto
            var productDto = _mapper.Map<ProductDto>(product);
            return Ok(productDto);
        }

        // API: POST api/Products
        // Tạo một Product mới, nhận vào Product Model, trả về ProductDto sau khi tạo
        [HttpPost]
        public async Task<ActionResult<ProductDto>> PostProduct(Product product) // Kiểu trả về là ProductDto
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Không cần LoadAsync() vì ProductDto không có Navigation Property cần tải
            // Nếu có, cần load ở đây

            // Ánh xạ Product Model vừa tạo sang ProductDto để trả về
            var productDto = _mapper.Map<ProductDto>(product);
            return CreatedAtAction(nameof(GetProduct), new { id = productDto.ProductID }, productDto);
        }

        // API: PUT api/Products/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(string id, Product product)
        {
            if (id != product.ProductID)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
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

        // API: DELETE api/Products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(string id)
        {
            return _context.Products.Any(e => e.ProductID == id);
        }
    }
}