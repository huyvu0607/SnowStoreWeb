using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SnowStoreWeb.Models
{
    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }  // Khóa chính, tự tăng

        [StringLength(500)]
        public string? ImageUrl { get; set; }  // Đường dẫn ảnh (nullable)

        
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; }  // Mô tả

        
        [Range(0, int.MaxValue, ErrorMessage = "Order must be a positive number")]
        public int Order { get; set; } // Thứ tự xuất hiện (tăng thủ công)
    }
}
