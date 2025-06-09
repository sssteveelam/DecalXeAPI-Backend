using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace DecalXeAPI.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next; // Đại diện cho middleware tiếp theo trong pipeline
        private readonly ILogger<ExceptionHandlingMiddleware> _logger; // Để ghi log lỗi

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        // Phương thức InvokeAsync sẽ được gọi khi request đi qua middleware này
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Gọi middleware tiếp theo trong pipeline.
                // Nếu không có lỗi, request sẽ tiếp tục bình thường.
                await _next(context);
            }
            catch (Exception ex) // Bắt tất cả các loại lỗi (Exception) xảy ra ở các middleware sau
            {
                // --- Xử lý lỗi ---
                _logger.LogError(ex, "Đã xảy ra lỗi không mong muốn: {ErrorMessage}", ex.Message); // Ghi log lỗi chi tiết

                context.Response.ContentType = "application/json"; // Đặt kiểu nội dung trả về là JSON
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError; // Đặt mã trạng thái HTTP là 500 Internal Server Error

                // Tạo đối tượng lỗi để trả về cho client
                var errorResponse = new
                {
                    StatusCode = context.Response.StatusCode,
                    Message = "Đã xảy ra lỗi nội bộ máy chủ. Vui lòng thử lại sau.", // Thông báo thân thiện cho người dùng
                    // Chi tiết lỗi chỉ nên trả về trong môi trường phát triển (Development)
                    // Details = ex.Message // KHÔNG NÊN HIỂN THỊ TRONG MÔI TRƯỜNG SẢN XUẤT (PRODUCTION)
                };

                // Chuyển đối tượng lỗi thành chuỗi JSON và ghi vào Response Body
                var jsonError = JsonSerializer.Serialize(errorResponse);
                await context.Response.WriteAsync(jsonError);
            }
        }
    }
}