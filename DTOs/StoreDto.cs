namespace DecalXeAPI.DTOs
{
    /// <summary>
    /// DTO đại diện cho thông tin cửa hàng.
    /// </summary>
    public class StoreDto
    {
        /// <summary>
        /// ID duy nhất của cửa hàng.
        /// </summary>
        /// <example>STORE_HCM_Q1</example>
        public string StoreID { get; set; } = string.Empty;

        /// <summary>
        /// Tên của cửa hàng.
        /// </summary>
        /// <example>Cửa hàng Decal Quận 1</example>
        public string StoreName { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ của cửa hàng.
        /// </summary>
        /// <example>123 Nguyễn Huệ, Quận 1, TP.HCM</example>
        public string? Address { get; set; } // <-- THÊM DÒNG NÀY
    }
}
