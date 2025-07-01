// DecalXeAPI/Models/Category.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DecalXeAPI.Models
{
    public class Category
    {
        [Key]
        public string CategoryID { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty; // Tên danh mục

        [MaxLength(500)]
        public string? Description { get; set; } // Mô tả

        // Navigation property: Một danh mục có thể chứa nhiều sản phẩm
        public ICollection<Product>? Products { get; set; }
    }
}