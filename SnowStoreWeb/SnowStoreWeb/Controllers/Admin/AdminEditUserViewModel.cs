namespace SnowStoreWeb.Controllers.Admin
{
    public class AdminEditUserViewModel
    {
        public int UserId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Role { get; set; }

        public DateTime CreatedDate { get; set; }

        // Password mới (nếu admin muốn đổi)
        public string? NewPassword { get; set; }

    }
}
