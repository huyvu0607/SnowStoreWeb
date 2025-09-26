using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnowStoreWeb.Attributes;
using SnowStoreWeb.Models;
using SnowStoreWeb.ViewModels.admin;

namespace SnowStoreWeb.Controllers_Admin
{
    [AuthorizeUser("Admin")]
    public class AdminUsersController : Controller
    {
        private readonly SnowStoreContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<AdminUsersController> _logger;

        public AdminUsersController(
            SnowStoreContext context,
            IPasswordHasher<User> passwordHasher,
            ILogger<AdminUsersController> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: AdminUsers
        public async Task<IActionResult> Index(string searchTerm = "", string roleFilter = "", int page = 1, int pageSize = 10)
        {
            var query = _context.Users.AsQueryable();

            // Search functionality
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u => u.Name.Contains(searchTerm) || u.Email.Contains(searchTerm));
            }

            // Role filter
            if (!string.IsNullOrEmpty(roleFilter))
            {
                query = query.Where(u => u.Role == roleFilter);
            }

            // Pagination
            var totalCount = await query.CountAsync();
            var users = await query
                .OrderBy(u => u.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.RoleFilter = roleFilter;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalCount = totalCount;

            // Role options for filter dropdown
            ViewBag.RoleOptions = GetRoleOptions();

            return View(users);
        }

        // GET: AdminUsers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: AdminUsers/Create
        public IActionResult Create()
        {
            ViewBag.RoleOptions = GetRoleOptions();
            return View(new AdminCreateUserViewModel());
        }

        // POST: AdminUsers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminCreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Validate password strength
                if (!IsValidPassword(model.Password))
                {
                    ModelState.AddModelError("Password", "Password must be at least 8 characters with uppercase, lowercase, number, and special character.");
                    ViewBag.RoleOptions = GetRoleOptions();
                    return View(model);
                }

                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email.ToLower() == model.Email.ToLower()))
                {
                    ModelState.AddModelError("Email", "Email already exists.");
                    ViewBag.RoleOptions = GetRoleOptions();
                    return View(model);
                }

                // Validate role
                var validRoles = GetValidRoles();
                if (!validRoles.Contains(model.Role))
                {
                    ModelState.AddModelError("Role", "Invalid role selected.");
                    ViewBag.RoleOptions = GetRoleOptions();
                    return View(model);
                }

                // Create new user
                var user = new User
                {
                    Name = model.Name.Trim(),
                    Email = model.Email.ToLower().Trim(),
                    PasswordHash = _passwordHasher.HashPassword(null, model.Password),
                    Role = model.Role,
                    CreatedDate = DateTime.UtcNow,

                };

                _context.Add(user);
                await _context.SaveChangesAsync();

                // Log admin action
                _logger.LogInformation("Admin {AdminEmail} created user {UserEmail} with role {Role}",
                    User.Identity.Name, user.Email, user.Role);

                TempData["SuccessMessage"] = "User created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.RoleOptions = GetRoleOptions();
            return View(model);
        }

        // GET: AdminUsers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new AdminEditUserViewModel
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                CreatedDate = (DateTime)user.CreatedDate
            };

            ViewBag.RoleOptions = GetRoleOptions();
            return View(model);
        }

        // POST: AdminUsers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminEditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.RoleOptions = new List<SelectListItem>
        {
            new SelectListItem { Text = "Admin", Value = "Admin" },
            new SelectListItem { Text = "User", Value = "User" }
        };
                return View(model);
            }

            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var currentUserId = HttpContext.Session.GetString("UserId");
            var isEditingSelf = user.UserId.ToString() == currentUserId;
            var isTargetUser = user.Role == "User";
            if (!(isEditingSelf || isTargetUser))
            {
                TempData["ErrorMessage"] = "You can only edit User accounts or your own account.";
                return RedirectToAction("Index");
            }

            user.Name = model.Name;
            user.Email = model.Email;
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, model.NewPassword);
            }
            if (!isEditingSelf) // Không cho phép thay đổi vai trò của chính mình
            {
                user.Role = model.Role;
            }

            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "User updated successfully.";
            return RedirectToAction("Index");
        }

        // GET: AdminUsers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: AdminUsers/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                // Ngăn admin tự xóa chính mình
                var currentUserEmail = User.Identity?.Name;
                if (user.Email == currentUserEmail)
                {
                    TempData["ErrorMessage"] = "You cannot delete your own account.";
                    return RedirectToAction(nameof(Index));
                }

                // Ngăn xóa Admin khác
                if (user.Role == "Admin")
                {
                    TempData["ErrorMessage"] = "Admin accounts cannot be deleted.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Admin {AdminEmail} deleted user {UserEmail}",
                    currentUserEmail, user.Email);

                TempData["SuccessMessage"] = "User deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }


        // AJAX endpoint to generate random password
        [HttpPost]
        public IActionResult GeneratePassword()
        {
            var password = GenerateSecurePassword();
            return Json(new { password = password });
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }

        private List<SelectListItem> GetRoleOptions()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "Select Role", Value = "", Disabled = true },
                new SelectListItem { Text = "User", Value = "User" },
                new SelectListItem { Text = "Admin", Value = "Admin" },
            };
        }

        private string[] GetValidRoles()
        {
            return new[] { "User", "Admin", "Manager", "Editor" };
        }

        private bool IsValidPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

        private string GenerateSecurePassword(int length = 12)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
            var random = new Random();
            var chars = new char[length];

            // Ensure at least one of each type
            chars[0] = validChars[random.Next(26, 52)]; // uppercase
            chars[1] = validChars[random.Next(0, 26)];  // lowercase
            chars[2] = validChars[random.Next(52, 62)]; // digit
            chars[3] = validChars[random.Next(62)];     // special

            // Fill the rest randomly
            for (int i = 4; i < length; i++)
            {
                chars[i] = validChars[random.Next(validChars.Length)];
            }

            // Shuffle array
            for (int i = 0; i < length; i++)
            {
                int j = random.Next(i, length);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }

            return new string(chars);
        }
    }
}