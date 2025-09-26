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
        // Trong action Index
        public async Task<IActionResult> Index(int page = 1, string category = null, string sortBy = null,
    decimal? minPrice = null, decimal? maxPrice = null, string brands = null)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .AsQueryable();

            // Apply filters
            query = ApplyFilters(query, category, minPrice, maxPrice, brands);

            // Apply sorting
            query = ApplySorting(query, sortBy);

            // Tổng sản phẩm trong DB (không filter)
            var allProductsCount = await _context.Products.CountAsync();

            // Tổng sản phẩm sau khi filter
            var totalProducts = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)_pageSize);

            var products = await query
                .Skip((page - 1) * _pageSize)
                .Take(_pageSize)
                .ToListAsync();

            // Gán vào ViewBag
            ViewBag.AllProductsCount = allProductsCount;   // 👈 tổng tất cả sản phẩm
            ViewBag.TotalProducts = totalProducts;         // 👈 tổng sản phẩm sau filter
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentSort = sortBy;
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Brands = await _context.Brands.Include(b => b.Products).OrderBy(b => b.Name).ToListAsync();

            // Price range data
            ViewBag.MinPrice = 0;
            ViewBag.MaxPrice = 1000000;
            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;

            // Brand data
            ViewBag.CurrentBrands = brands ?? "";

            return View(products);
        }

        // GET: GetMoreProducts - Updated for Load More functionality
        [HttpGet]
        public async Task<IActionResult> GetMoreProducts(int page = 2, string category = null, string sortBy = null,
            decimal? minPrice = null, decimal? maxPrice = null, string brands = null)
        {
            try
            {
                var query = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .AsQueryable();

                // Apply filters
                query = ApplyFilters(query, category, minPrice, maxPrice, brands);

                // Apply sorting
                query = ApplySorting(query, sortBy);

                // Calculate pagination
                var totalProducts = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalProducts / (double)_pageSize);
                var startIndex = (page - 1) * _pageSize;

                // Check if page is valid
                if (page > totalPages || page < 1)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Trang không hợp lệ",
                        products = new List<object>(),
                        totalProducts = 0,
                        currentPage = page,
                        totalPages = 0,
                        hasMore = false
                    });
                }

                var products = await query
                    .Skip(startIndex)
                    .Take(_pageSize)
                    .Select(p => new
                    {
                        id = p.ProductId,
                        name = p.Name,
                        price = p.Price.ToString("N0"),
                        image = p.ImageUrl,
                        category = p.Category != null ? p.Category.Name : "Không phân loại",
                        brand = p.Brand != null ? p.Brand.Name : "Không rõ thương hiệu",
                        isHot = p.IsHot ?? false,
                        isBestSeller = p.IsBestSeller ?? false,
                        stockQuantity = p.StockQuantity ?? 0,
                        url = Url.Action("Details", "Product", new { id = p.ProductId })
                    })
                    .ToListAsync();

                var hasMore = page < totalPages;

                return Json(new
                {
                    success = true,
                    products,
                    totalProducts,
                    currentPage = page,
                    totalPages,
                    hasMore,
                    message = products.Any() ? "Tải thành công" : "Không có sản phẩm nào"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi tải sản phẩm",
                    error = ex.Message,
                    products = new List<object>(),
                    totalProducts = 0,
                    currentPage = page,
                    totalPages = 0,
                    hasMore = false
                });
            }
        }

        // GET: Product/Search - Updated for Load More support
        public async Task<IActionResult> Search(string query, int page = 1, string category = null,
            string sortBy = null, decimal? minPrice = null, decimal? maxPrice = null, string brands = null)
        {
            var searchQuery = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .AsQueryable();

            // Apply search filters
            searchQuery = ApplySearchFilters(searchQuery, query, category, minPrice, maxPrice, brands);

            // Apply sorting
            searchQuery = ApplySorting(searchQuery, sortBy);

            // Pagination
            var totalProducts = await searchQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)_pageSize);

            var allProductsCount = await _context.Products.CountAsync();
            var products = await searchQuery
                .Skip((page - 1) * _pageSize)
                .Take(_pageSize)
                .ToListAsync();

            // Pass search data to view
            ViewBag.AllProductsCount = allProductsCount;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.SearchQuery = query;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentSort = sortBy;
            ViewBag.TotalResults = totalProducts;
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Brands = await _context.Brands.Include(b => b.Products).OrderBy(b => b.Name).ToListAsync();

            // Price range data
            ViewBag.MinPrice = 0;
            ViewBag.MaxPrice = 1000000;
            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;

            // Brand data
            ViewBag.CurrentBrands = brands ?? "";

            // For breadcrumb and page title
            ViewData["Title"] = !string.IsNullOrEmpty(query) ? $"Kết quả tìm kiếm: \"{query}\"" : "Tìm kiếm sản phẩm";

            return View("Index", products);
        }

        // GET: GetMoreSearchResults - New action for load more in search results
        [HttpGet]
        public async Task<IActionResult> GetMoreSearchResults(string query, int page = 2, string category = null,
            string sortBy = null, decimal? minPrice = null, decimal? maxPrice = null, string brands = null)
        {
            try
            {
                var searchQuery = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .AsQueryable();

                // Apply search filters
                searchQuery = ApplySearchFilters(searchQuery, query, category, minPrice, maxPrice, brands);

                // Apply sorting
                searchQuery = ApplySorting(searchQuery, sortBy);

                // Calculate pagination
                var totalProducts = await searchQuery.CountAsync();
                var totalPages = (int)Math.Ceiling(totalProducts / (double)_pageSize);
                var startIndex = (page - 1) * _pageSize;

                // Check if page is valid
                if (page > totalPages || page < 1)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Trang không hợp lệ",
                        products = new List<object>(),
                        totalProducts = 0,
                        currentPage = page,
                        totalPages = 0,
                        hasMore = false
                    });
                }

                var products = await searchQuery
                    .Skip(startIndex)
                    .Take(_pageSize)
                    .Select(p => new
                    {
                        id = p.ProductId,
                        name = p.Name,
                        price = p.Price.ToString("N0"),
                        image = p.ImageUrl,
                        category = p.Category != null ? p.Category.Name : "Không phân loại",
                        brand = p.Brand != null ? p.Brand.Name : "Không rõ thương hiệu",
                        isHot = p.IsHot ?? false,
                        isBestSeller = p.IsBestSeller ?? false,
                        stockQuantity = p.StockQuantity ?? 0,
                        url = Url.Action("Details", "Product", new { id = p.ProductId })
                    })
                    .ToListAsync();

                var hasMore = page < totalPages;

                return Json(new
                {
                    success = true,
                    products,
                    totalProducts,
                    currentPage = page,
                    totalPages,
                    hasMore,
                    searchQuery = query,
                    message = products.Any() ? "Tải thành công" : "Không có sản phẩm nào"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi tải kết quả tìm kiếm",
                    error = ex.Message,
                    products = new List<object>(),
                    totalProducts = 0,
                    currentPage = page,
                    totalPages = 0,
                    hasMore = false
                });
            }
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
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.Name.ToLower().Contains(term.ToLower()) ||
                           (p.Description != null && p.Description.ToLower().Contains(term.ToLower())) ||
                           (p.Brand != null && p.Brand.Name.ToLower().Contains(term.ToLower())))
                .Select(p => new
                {
                    id = p.ProductId,
                    name = p.Name,
                    price = p.Price,
                    image = p.ImageUrl,
                    category = p.Category != null ? p.Category.Name : "Không phân loại",
                    brand = p.Brand != null ? p.Brand.Name : "Không rõ thương hiệu"
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
                .Include(p => p.Brand)
                .Where(p => p.Name.ToLower().Contains(query.ToLower()) ||
                           (p.Description != null && p.Description.ToLower().Contains(query.ToLower())) ||
                           (p.Category != null && p.Category.Name.ToLower().Contains(query.ToLower())) ||
                           (p.Brand != null && p.Brand.Name.ToLower().Contains(query.ToLower())))
                .Select(p => new
                {
                    id = p.ProductId,
                    name = p.Name,
                    price = p.Price.ToString("N0"),
                    image = p.ImageUrl,
                    category = p.Category != null ? p.Category.Name : "Không phân loại",
                    brand = p.Brand != null ? p.Brand.Name : "Không rõ thương hiệu",
                    url = Url.Action("Details", "Product", new { id = p.ProductId })
                })
                .Take(10)
                .ToListAsync();

            return Json(new { success = true, products });
        }

        // GET: Product/GetFilteredProducts (AJAX endpoint for dynamic filtering)
        [HttpGet]
        public async Task<IActionResult> GetFilteredProducts(string category = null, decimal? minPrice = null,
            decimal? maxPrice = null, string brands = null, string sortBy = null, int page = 1)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .AsQueryable();

            // Apply filters
            query = ApplyFilters(query, category, minPrice, maxPrice, brands);

            // Apply sorting
            query = ApplySorting(query, sortBy);

            // Pagination
            var totalProducts = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)_pageSize);

            var products = await query
                .Skip((page - 1) * _pageSize)
                .Take(_pageSize)
                .Select(p => new
                {
                    id = p.ProductId,
                    name = p.Name,
                    price = p.Price.ToString("N0"),
                    image = p.ImageUrl,
                    category = p.Category != null ? p.Category.Name : "Không phân loại",
                    brand = p.Brand != null ? p.Brand.Name : "Không rõ thương hiệu",
                    isHot = p.IsHot ?? false,
                    isBestSeller = p.IsBestSeller ?? false,
                    stockQuantity = p.StockQuantity ?? 0,
                    url = Url.Action("Details", "Product", new { id = p.ProductId })
                })
                .ToListAsync();

            return Json(new
            {
                success = true,
                products,
                totalProducts,
                totalPages,
                currentPage = page
            });
        }

        private IQueryable<Product> ApplyFilters(IQueryable<Product> query, string category = null,
    decimal? minPrice = null, decimal? maxPrice = null, string brands = null)
        {
            // Category filter
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category != null && p.Category.Name.ToLower() == category.ToLower());
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

            // Brand filter
            if (!string.IsNullOrWhiteSpace(brands))
            {
                var brandList = brands.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                     .Select(b => b.Trim().ToLower())
                                     .Where(b => !string.IsNullOrEmpty(b))
                                     .ToList();

                if (brandList.Any())
                {
                    query = query.Where(p => p.Brand != null && brandList.Contains(p.Brand.Name.ToLower()));
                }
            }

            return query;
        }

        private IQueryable<Product> ApplySearchFilters(IQueryable<Product> query, string searchTerm,
            string category = null, decimal? minPrice = null, decimal? maxPrice = null, string brands = null)
        {
            // Text search
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var terms = searchTerm.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (var term in terms)
                {
                    query = query.Where(p =>
                        p.Name.ToLower().Contains(term) ||
                        (p.Description != null && p.Description.ToLower().Contains(term)) ||
                        (p.Category != null && p.Category.Name.ToLower().Contains(term)) ||
                        (p.Brand != null && p.Brand.Name.ToLower().Contains(term)));
                }
            }

            // Apply other filters
            query = ApplyFilters(query, category, minPrice, maxPrice, brands);

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
                "brand_asc" => query.OrderBy(p => p.Brand != null ? p.Brand.Name : ""),
                "brand_desc" => query.OrderByDescending(p => p.Brand != null ? p.Brand.Name : ""),
                "hot" => query.OrderByDescending(p => p.IsHot).ThenByDescending(p => p.IsBestSeller),
                "bestseller" => query.OrderByDescending(p => p.IsBestSeller).ThenByDescending(p => p.IsHot),
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
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            // Cải thiện logic lấy sản phẩm liên quan
            var relatedProducts = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.ProductId != product.ProductId) // Lấy tất cả sản phẩm khác
                .OrderBy(p => p.CategoryId == product.CategoryId ? 0 : 1) // Ưu tiên cùng danh mục
                .ThenBy(p => p.BrandId == product.BrandId ? 0 : 1) // Ưu tiên cùng thương hiệu
                .ThenByDescending(p => p.IsHot)
                .ThenByDescending(p => p.IsBestSeller)
                .Take(8) // Tăng số lượng
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;
            ViewBag.RelatedProductsCount = relatedProducts.Count;

            // Breadcrumb data
            ViewBag.CategoryName = product.Category?.Name;
            ViewBag.BrandName = product.Brand?.Name;

            return View(product);
        }

        // GET: Product/Category/CategoryName
        public async Task<IActionResult> Category(string categoryName, int page = 1, string sortBy = null,
            decimal? minPrice = null, decimal? maxPrice = null, string brands = null)
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

            return await Index(page, categoryName, sortBy, minPrice, maxPrice, brands);
        }

        // GET: Product/Brand/BrandName
        public async Task<IActionResult> Brand(string brandName, int page = 1, string sortBy = null,
            decimal? minPrice = null, decimal? maxPrice = null, string category = null)
        {
            if (string.IsNullOrWhiteSpace(brandName))
            {
                return RedirectToAction("Index");
            }

            var brand = await _context.Brands
                .FirstOrDefaultAsync(b => b.Name.ToLower() == brandName.ToLower());

            if (brand == null)
            {
                return NotFound();
            }

            return await Index(page, category, sortBy, minPrice, maxPrice, brandName);
        }

        // GET: Product/GetPriceRange (AJAX endpoint to get price range for specific filters)
        [HttpGet]
        public async Task<IActionResult> GetPriceRange(string category = null, string brands = null)
        {
            var query = _context.Products.AsQueryable();

            // Apply filters except price
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category != null && p.Category.Name.ToLower() == category.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(brands))
            {
                var brandList = brands.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                     .Select(b => b.Trim().ToLower())
                                     .Where(b => !string.IsNullOrEmpty(b))
                                     .ToList();

                if (brandList.Any())
                {
                    query = query.Where(p => p.Brand != null && brandList.Contains(p.Brand.Name.ToLower()));
                }
            }

            var prices = await query.Select(p => p.Price).ToListAsync();

            var result = new
            {
                minPrice = 0,
                maxPrice = 1000000,
                count = prices.Count
            };

            return Json(result);
        }

        // GET: Product/GetBrandCounts (AJAX endpoint to get product counts for each brand)
        [HttpGet]
        public async Task<IActionResult> GetBrandCounts(string category = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            var query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .AsQueryable();

            // Apply filters except brands
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category != null && p.Category.Name.ToLower() == category.ToLower());
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            var brandCounts = await query
                .GroupBy(p => new { p.BrandId, BrandName = p.Brand != null ? p.Brand.Name : "Không rõ thương hiệu" })
                .Select(g => new
                {
                    brandId = g.Key.BrandId,
                    brandName = g.Key.BrandName,
                    count = g.Count()
                })
                .OrderBy(b => b.brandName)
                .ToListAsync();

            return Json(brandCounts);
        }
    }
}