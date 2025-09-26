using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnowStoreWeb.Models;
using SnowStoreWeb.ViewModels;

namespace SnowStoreWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly SnowStoreContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public AccountController(SnowStoreContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }
        public async Task<IActionResult> CreateAdmin()
        {
            if (!await _context.Users.AnyAsync(u => u.Role == "Admin"))
            {
                var admin = new User
                {
                    Name = "Admin",
                    Email = "admin@snowstore.com",
                    Role = "Admin",
                    CreatedDate = DateTime.Now
                };

                admin.PasswordHash = _passwordHasher.HashPassword(admin, "Admin@123");

                _context.Users.Add(admin);
                await _context.SaveChangesAsync();

                return Content("✅ Tài khoản Admin đã được tạo: admin@snowstore.com / Admin@123");
            }
            return Content("⚠️ Admin đã tồn tại!");
        }
        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại!");
                    return View(model);
                }

                var user = new User
                {
                    Name = model.Name,
                    Email = model.Email,
                    PasswordHash = _passwordHasher.HashPassword(null!, model.Password),
                    Role = "User",
                    CreatedDate = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("UserRole", user.Role ?? "User");

                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }


        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng!");
                return View();
            }

            // Kiểm tra mật khẩu
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng!");
                return View();
            }

            // Lưu session
            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserRole", user.Role);

            // ✅ Điều hướng theo Role
            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Admin"); // Layout Admin
            }
            else
            {
                return RedirectToAction("Index", "Home"); // Layout User
            }
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
