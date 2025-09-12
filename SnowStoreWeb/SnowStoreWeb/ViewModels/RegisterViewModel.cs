using System.ComponentModel.DataAnnotations;

namespace SnowStoreWeb.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Tên không được để trống")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }
}
