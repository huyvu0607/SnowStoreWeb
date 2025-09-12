using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnowStoreWeb.Attributes;
using SnowStoreWeb.Models;

namespace SnowStoreWeb.Controllers
{
    [AuthorizeUser("Admin")] // chỉ Admin mới vào
    public class AdminController : Controller
    {
        private readonly SnowStoreContext _context;

        public AdminController(SnowStoreContext context)
        {
            _context = context;
        }

        // Dashboard
        public IActionResult Index()
        {
            var productCount = _context.Products.Count();
            var userCount = _context.Users.Count();
            var hotProducts = _context.Products.Where(p => p.IsHot == true).Take(5).ToList();

            ViewBag.ProductCount = productCount;
            ViewBag.UserCount = userCount;
            ViewBag.HotProducts = hotProducts;

            return View();
        }

        // Quản lý sản phẩm
        public async Task<IActionResult> Products()
        {
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            return View(products);
        }

        // Quản lý user
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }
    }
}
