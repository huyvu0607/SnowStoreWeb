using System.ComponentModel.DataAnnotations;
namespace SnowStoreWeb.Models
{
    public class PopupBanner
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [Display(Name = "Tiêu đề")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; }

        [Display(Name = "URL Hình ảnh")]
        [Url(ErrorMessage = "URL hình ảnh không hợp lệ")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Mô tả")]
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? Description { get; set; }

        [Display(Name = "Thứ tự hiển thị")]
        [Range(1, 999, ErrorMessage = "Thứ tự phải từ 1 đến 999")]
        public int DisplayOrder { get; set; } = 1;

        [Display(Name = "Trạng thái")]
        public PopupStatus Status { get; set; } = PopupStatus.Inactive; // Mặc định là Inactive

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedAt { get; set; }
    }

    public enum PopupStatus
    {
        [Display(Name = "Đang hiển thị")]
        Active = 1,
        [Display(Name = "Chưa được hiển thị")]
        Inactive = 2
    }
}