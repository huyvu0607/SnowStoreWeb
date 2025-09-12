using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnowStoreWeb.Models;
using System.Linq;

namespace SnowStoreWeb.Controllers
{
    public class ProductController : Controller
    {
        private readonly SnowStoreContext _context;
        private readonly int _pageSize = 12; // Number of products per page

        public ProductController(SnowStoreContext context)
        {
            _context = context;
        }

        // GET: Product
        public async Task<IActionResult> Index(int page = 1, string category = null, string sortBy = null)
        {
            var query = _context.Products.Include(p => p.Category).AsQueryable();

            // Filter by category if specified
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category.Name.ToLower() == category.ToLower());
            }

            // Apply sorting
            query = ApplySorting(query, sortBy);

            // Pagination
            var totalProducts = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)_pageSize);

            var products = await query
                .Skip((page - 1) * _pageSize)
                .Take(_pageSize)
                .ToListAsync();

            // Pass data to view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentSort = sortBy;
            ViewBag.Categories = await _context.Categories.ToListAsync();

            return View(products);
        }

        // GET: Product/Search
        public async Task<IActionResult> Search(string query, int page = 1, string category = null, string sortBy = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction("Index");
            }

            var searchQuery = _context.Products.Include(p => p.Category).AsQueryable();

            // Apply search filters
            searchQuery = ApplySearchFilters(searchQuery, query, category, minPrice, maxPrice);

            // Apply sorting
            searchQuery = ApplySorting(searchQuery, sortBy);

            // Pagination
            var totalProducts = await searchQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)_pageSize);

            var products = await searchQuery
                .Skip((page - 1) * _pageSize)
                .Take(_pageSize)
                .ToListAsync();

            // Pass search data to view
            ViewBag.SearchQuery = query;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentSort = sortBy;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.TotalResults = totalProducts;
            ViewBag.Categories = await _context.Categories.ToListAsync();

            // For breadcrumb and page title
            ViewData["Title"] = $"Kết quả tìm kiếm: \"{query}\"";

            return View("Search", products);
        }

        // GET: Product/Suggestions (AJAX endpoint for search suggestions)
        [HttpGet]
        public async Task<IActionResult> Suggestions(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            {
                return Json(new List<object>());
            }

            var suggestions = await _context.Products
                .Where(p => p.Name.ToLower().Contains(term.ToLower()) ||
                           p.Description.ToLower().Contains(term.ToLower()))
                .Select(p => new
                {
                    id = p.ProductId,
                    name = p.Name,
                    price = p.Price,
                    image = p.ImageUrl,
                    category = p.Category.Name
                })
                .Take(8)
                .ToListAsync();

            return Json(suggestions);
        }

        // GET: Product/QuickSearch (AJAX endpoint for quick search)
        [HttpGet]
        public async Task<IActionResult> QuickSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new { success = false, message = "Query is required" });
            }

            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Name.ToLower().Contains(query.ToLower()) ||
                           p.Description.ToLower().Contains(query.ToLower()) ||
                           p.Category.Name.ToLower().Contains(query.ToLower()))
                .Select(p => new
                {
                    id = p.ProductId,
                    name = p.Name,
                    price = p.Price.ToString("N0"),
                    image = p.ImageUrl,
                    category = p.Category.Name,
                    url = Url.Action("Details", "Product", new { id = p.ProductId })
                })
                .Take(10)
                .ToListAsync();

            return Json(new { success = true, products });
        }

        private IQueryable<Product> ApplySearchFilters(IQueryable<Product> query, string searchTerm, string category = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            // Text search
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var terms = searchTerm.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (var term in terms)
                {
                    query = query.Where(p =>
                        p.Name.ToLower().Contains(term) ||
                        p.Description.ToLower().Contains(term) ||
                        p.Category.Name.ToLower().Contains(term));
                }
            }

            // Category filter
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category.Name.ToLower() == category.ToLower());
            }

            // Price range filter
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            return query;
        }

        private IQueryable<Product> ApplySorting(IQueryable<Product> query, string sortBy)
        {
            return sortBy?.ToLower() switch
            {
                "name_asc" => query.OrderBy(p => p.Name),
                "name_desc" => query.OrderByDescending(p => p.Name),
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "newest" => query.OrderByDescending(p => p.CreatedDate),
                "oldest" => query.OrderBy(p => p.CreatedDate),
                _ => query.OrderBy(p => p.Name) // Default sorting
            };
        }


        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            // Cải thiện logic lấy sản phẩm khác
            var relatedProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.ProductId != product.ProductId) // Lấy tất cả sản phẩm khác
                .OrderBy(p => p.CategoryId == product.CategoryId ? 0 : 1) // Ưu tiên cùng danh mục
                .ThenByDescending(p => p.IsHot)
                .ThenByDescending(p => p.IsBestSeller)
                .Take(8) // Tăng số lượng
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;

            // Debug
            ViewBag.RelatedProductsCount = relatedProducts.Count;

            return View(product);
        }

        // GET: Product/Category/CategoryName
        public async Task<IActionResult> Category(string categoryName, int page = 1, string sortBy = null)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return RedirectToAction("Index");
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == categoryName.ToLower());

            if (category == null)
            {
                return NotFound();
            }

            return await Index(page, categoryName, sortBy);
        }
    }
}