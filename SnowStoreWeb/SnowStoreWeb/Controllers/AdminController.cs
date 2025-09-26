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

        // Dashboard với thống kê chi tiết
        public async Task<IActionResult> Index()
        {
            // Thống kê tổng quan
            var productCount = await _context.Products.CountAsync();
            var userCount = await _context.Users.CountAsync();
            var categoryCount = await _context.Categories.CountAsync();
            var brandCount = await _context.Brands.CountAsync();

            // Thống kê sản phẩm
            var hotProductsCount = await _context.Products.CountAsync(p => p.IsHot == true);
            var bestSellerCount = await _context.Products.CountAsync(p => p.IsBestSeller == true);
            var outOfStockCount = await _context.Products.CountAsync(p => p.StockQuantity <= 0);
            var lowStockCount = await _context.Products.CountAsync(p => p.StockQuantity > 0 && p.StockQuantity <= 10);

            // Top sản phẩm HOT
            var hotProducts = await _context.Products
                .Where(p => p.IsHot == true)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .OrderByDescending(p => p.CreatedDate)
                .Take(5)
                .ToListAsync();

            // Top sản phẩm Best Seller
            var bestSellers = await _context.Products
                .Where(p => p.IsBestSeller == true)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .OrderByDescending(p => p.CreatedDate)
                .Take(5)
                .ToListAsync();

            // Thống kê theo danh mục
            var categoryStats = await _context.Categories
                .Select(c => new {
                    CategoryName = c.Name,
                    ProductCount = c.Products.Count(),
                    HotProductCount = c.Products.Count(p => p.IsHot == true),
                    BestSellerCount = c.Products.Count(p => p.IsBestSeller == true)
                })
                .Where(c => c.ProductCount > 0)
                .OrderByDescending(c => c.ProductCount)
                .Take(5)
                .ToListAsync();

            // Thống kê theo thương hiệu
            var brandStats = await _context.Brands
                .Select(b => new {
                    BrandName = b.Name,
                    ProductCount = b.Products.Count(),
                    HotProductCount = b.Products.Count(p => p.IsHot == true),
                    BestSellerCount = b.Products.Count(p => p.IsBestSeller == true)
                })
                .Where(b => b.ProductCount > 0)
                .OrderByDescending(b => b.ProductCount)
                .Take(5)
                .ToListAsync();

            // Thống kê người dùng theo thời gian (7 ngày gần nhất)
            var last7Days = DateTime.Now.AddDays(-7);
            var newUsersLast7Days = await _context.Users
                .Where(u => u.CreatedDate >= last7Days)
                .CountAsync();

            // Thống kê sản phẩm mới (30 ngày gần nhất)
            var last30Days = DateTime.Now.AddDays(-30);
            var newProductsLast30Days = await _context.Products
                .Where(p => p.CreatedDate >= last30Days)
                .CountAsync();

            // Sản phẩm có giá cao nhất và thấp nhất
            var highestPriceProduct = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .OrderByDescending(p => p.Price)
                .FirstOrDefaultAsync();

            var lowestPriceProduct = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.Price > 0)
                .OrderBy(p => p.Price)
                .FirstOrDefaultAsync();

            // Truyền dữ liệu qua ViewBag
            ViewBag.ProductCount = productCount;
            ViewBag.UserCount = userCount;
            ViewBag.CategoryCount = categoryCount;
            ViewBag.BrandCount = brandCount;
            ViewBag.HotProductsCount = hotProductsCount;
            ViewBag.BestSellerCount = bestSellerCount;
            ViewBag.OutOfStockCount = outOfStockCount;
            ViewBag.LowStockCount = lowStockCount;
            ViewBag.NewUsersLast7Days = newUsersLast7Days;
            ViewBag.NewProductsLast30Days = newProductsLast30Days;

            ViewBag.HotProducts = hotProducts;
            ViewBag.BestSellers = bestSellers;
            ViewBag.CategoryStats = categoryStats;
            ViewBag.BrandStats = brandStats;
            ViewBag.HighestPriceProduct = highestPriceProduct;
            ViewBag.LowestPriceProduct = lowestPriceProduct;

            return View();
        }

        // Quản lý sản phẩm
        public async Task<IActionResult> Products()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
            return View(products);
        }

        // Quản lý user
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedDate)
                .ToListAsync();
            return View(users);
        }
    }
}