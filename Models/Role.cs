using System.ComponentModel.DataAnnotations; // Để dùng thuộc tính [Key] và [Required]
using System.ComponentModel.DataAnnotations.Schema; // Để dùng [Column] nếu cần kiểu dữ liệu cụ thể cho DB
using System.Text.Json.Serialization; // <-- THÊM DÒNG NÀY

namespace DecalXeAPI.Models
{
    public class Role
    {
        [Key] // Thuộc tính [Key] đánh dấu đây là khóa chính của bảng.
        // Đệ có thể dùng Guid.NewGuid().ToString() để tạo ID tự động,
        // hoặc nếu ID là chuỗi ngắn cố định (ví dụ: "ADMIN", "CUST") thì có thể gán thủ công sau.
        public string RoleID { get; set; } = Guid.NewGuid().ToString(); 

        [Required] // Thuộc tính [Required] đánh dấu trường này là bắt buộc (NOT NULL) trong database.
        [MaxLength(50)] // Giới hạn độ dài tối đa của chuỗi là 50 ký tự.
        public string RoleName { get; set; } = string.Empty; // Tên vai trò (ví dụ: "Admin", "Customer", "Technician")

        // Navigation Property (quan hệ một-nhiều): Một Role có thể có nhiều Accounts.
        // Mặc dù bây giờ chưa có Account, nhưng mình cứ khai báo sẵn để sau này dễ dàng liên kết.
        // Dấu '?' sau ICollection<Account> có nghĩa là tập hợp này có thể rỗng (null)
        // trước khi có bất kỳ Account nào được liên kết.
        [JsonIgnore] 
        public ICollection<Account>? Accounts { get; set; }
    }
}