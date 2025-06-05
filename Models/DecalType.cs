using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DecalXeAPI.Models
{
    public class DecalType
    {
        [Key]
        public string DecalTypeID { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(100)]
        public string DecalTypeName { get; set; } = string.Empty; // Ví dụ: "Decal Carbon", "Decal Chrome"

        [MaxLength(100)]
        public string? Material { get; set; } // Ví dụ: "Vinyl", "PVC"

        public decimal? Width { get; set; } // Chiều rộng, dùng decimal để chính xác số thập phân
        public decimal? Height { get; set; } // Chiều cao

        // Navigation Property: Một DecalType có thể được dùng trong nhiều DecalService.
        public ICollection<DecalService>? DecalServices { get; set; }
    }
}