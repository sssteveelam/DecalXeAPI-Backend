// DecalXeAPI/Models/VehiclePart.cs
using System.ComponentModel;

namespace DecalXeAPI.Models
{
    /// <summary>
    /// Enum định nghĩa các vị trí có thể đặt decal trên xe
    /// </summary>
    public enum VehiclePart
    {
        [Description("Nắp capô")]
        Hood = 1,           // Nắp capô phía trước

        [Description("Nóc xe")]
        Roof = 2,           // Nóc xe

        [Description("Cốp xe")]
        Trunk = 3,          // Cốp sau hoặc cửa cốp

        [Description("Cản trước")]
        FrontBumper = 4,    // Cản trước

        [Description("Cản sau")]
        RearBumper = 5,     // Cản sau

        [Description("Cửa bên")]
        SideDoor = 6,       // Cửa bên trái hoặc phải

        [Description("Chắn bùn")]
        Fender = 7,         // Chắn bùn trước hoặc sau

        [Description("Khác")]
        Other = 8           // Vị trí không chuẩn khác
    }
}
