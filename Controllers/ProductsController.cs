using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API: GET api/Products
        // Lấy tất cả các Product có trong database
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        // API: GET api/Products/{id}
        // Lấy thông tin một Product theo ProductID
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(string id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound(); // Trả về lỗi 404 Not Found
            }

            return product; // Trả về Product tìm được
        }

        // API: POST api/Products
        // Tạo một Product mới
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            _context.Products.Add(product); // Thêm Product mới vào DbSet
            await _context.SaveChangesAsync(); // Lưu các thay đổi vào database

            // Trả về kết quả 201 Created và thông tin của Product vừa tạo
            return CreatedAtAction(nameof(GetProduct), new { id = product.ProductID }, product);
        }

        // API: PUT api/Products/{id}
        // Cập nhật thông tin một Product hiện có
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(string id, Product product)
        {
            // Kiểm tra xem ID trong đường dẫn có khớp với ProductID trong body request không
            if (id != product.ProductID)
            {
                return BadRequest(); // Trả về lỗi 400 Bad Request nếu không khớp
            }

            _context.Entry(product).State = EntityState.Modified; // Đánh dấu Entity là đã được Modified

            try
            {
                await _context.SaveChangesAsync(); // Lưu các thay đổi vào database
            }
            catch (DbUpdateConcurrencyException) // Xử lý lỗi nếu có xung đột cập nhật
            {
                if (!ProductExists(id)) // Kiểm tra xem Product có tồn tại không
                {
                    return NotFound(); // Nếu không tồn tại, trả về 404 Not Found
                }
                else
                {
                    throw; // Nếu là lỗi khác, ném lại lỗi
                }
            }

            return NoContent(); // Trả về 204 No Content (cập nhật thành công nhưng không có nội dung trả về)
        }

        // API: DELETE api/Products/{id}
        // Xóa một Product
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            var product = await _context.Products.FindAsync(id); // Tìm Product cần xóa
            if (product == null)
            {
                return NotFound(); // Nếu không tìm thấy
            }

            _context.Products.Remove(product); // Xóa Product khỏi DbSet
            await _context.SaveChangesAsync(); // Lưu thay đổi vào database

            return NoContent(); // Trả về 204 No Content
        }

        // Hàm hỗ trợ: Kiểm tra xem Product có tồn tại không
        private bool ProductExists(string id)
        {
            return _context.Products.Any(e => e.ProductID == id);
        }
    }
}