using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DecalXeAPI.Data;
using DecalXeAPI.Models;

namespace DecalXeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServiceProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API: GET api/ServiceProducts
        // Lấy tất cả các ServiceProduct, bao gồm thông tin DecalService và Product liên quan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceProduct>>> GetServiceProducts()
        {
            // .Include() để tải dữ liệu của cả DecalService và Product liên quan
            return await _context.ServiceProducts
                                .Include(sp => sp.DecalService)
                                .Include(sp => sp.Product)
                                .ToListAsync();
        }

        // API: GET api/ServiceProducts/{id}
        // Lấy thông tin một ServiceProduct theo ServiceProductID, bao gồm các thông tin liên quan
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceProduct>> GetServiceProduct(string id)
        {
            var serviceProduct = await _context.ServiceProducts
                                                .Include(sp => sp.DecalService)
                                                .Include(sp => sp.Product)
                                                .FirstOrDefaultAsync(sp => sp.ServiceProductID == id);

            if (serviceProduct == null)
            {
                return NotFound();
            }

            return serviceProduct;
        }

        // API: POST api/ServiceProducts
        // Tạo một ServiceProduct mới
        [HttpPost]
        public async Task<ActionResult<ServiceProduct>> PostServiceProduct(ServiceProduct serviceProduct)
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

            // Tải lại thông tin liên quan để trả về đầy đủ
            await _context.Entry(serviceProduct).Reference(sp => sp.DecalService).LoadAsync();
            await _context.Entry(serviceProduct).Reference(sp => sp.Product).LoadAsync();

            return CreatedAtAction(nameof(GetServiceProduct), new { id = serviceProduct.ServiceProductID }, serviceProduct);
        }

        // API: PUT api/ServiceProducts/{id}
        // Cập nhật thông tin một ServiceProduct hiện có
        [HttpPut("{id}")]
        public async Task<IActionResult> PutServiceProduct(string id, ServiceProduct serviceProduct)
        {
            if (id != serviceProduct.ServiceProductID)
            {
                return BadRequest();
            }

            // Kiểm tra xem ServiceID và ProductID có tồn tại không trước khi cập nhật
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
        // Xóa một ServiceProduct
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

        // Hàm hỗ trợ: Kiểm tra xem ServiceProduct có tồn tại không
        private bool ServiceProductExists(string id)
        {
            return _context.ServiceProducts.Any(e => e.ServiceProductID == id);
        }

        // Hàm hỗ trợ: Kiểm tra xem DecalService có tồn tại không (copy từ DecalServicesController)
        private bool DecalServiceExists(string id)
        {
            return _context.DecalServices.Any(e => e.ServiceID == id);
        }

        // Hàm hỗ trợ: Kiểm tra xem Product có tồn tại không (copy từ ProductsController)
        private bool ProductExists(string id)
        {
            return _context.Products.Any(e => e.ProductID == id);
        }
    }
}