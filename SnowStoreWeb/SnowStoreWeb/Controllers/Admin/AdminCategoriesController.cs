using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnowStoreWeb.Attributes;
using SnowStoreWeb.Models;

namespace SnowStoreWeb.Controllers.Admin
{
    [AuthorizeUser("Admin")]
    public class AdminCategoriesController : Controller
    {
        private readonly SnowStoreContext _context;

        public AdminCategoriesController(SnowStoreContext context)
        {
            _context = context;
        }

        // GET: Admin/AdminCategories
        public async Task<IActionResult> Index(string searchString, string sortOrder, int? pageNumber)
        {
            // ViewData for sorting
            ViewData["CurrentFilter"] = searchString;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["IdSortParm"] = sortOrder == "Id" ? "id_desc" : "Id";
            ViewData["CurrentSort"] = sortOrder;

            var categories = from c in _context.Categories
                             select c;

            // Search functionality
            if (!String.IsNullOrEmpty(searchString))
            {
                categories = categories.Where(c => c.Name.Contains(searchString)
                                                || (c.Description != null && c.Description.Contains(searchString)));
            }

            // Sorting functionality
            switch (sortOrder)
            {
                case "name_desc":
                    categories = categories.OrderByDescending(c => c.Name);
                    break;
                case "Id":
                    categories = categories.OrderBy(c => c.CategoryId);
                    break;
                case "id_desc":
                    categories = categories.OrderByDescending(c => c.CategoryId);
                    break;
                default:
                    categories = categories.OrderBy(c => c.Name);
                    break;
            }

            var categoriesList = await categories.ToListAsync();

            // Statistics for dashboard
            ViewData["TotalCategories"] = categoriesList.Count();
            ViewData["SearchResultsCount"] = categoriesList.Count();

            return View(categoriesList);
        }

        // GET: Admin/AdminCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryId == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Admin/AdminCategories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/AdminCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Category created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Admin/AdminCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Admin/AdminCategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,Name,Description")] Category category)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Category updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CategoryId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // POST: Admin/AdminCategories/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products) // Include related products
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category != null)
            {
                try
                {
                    // Set CategoryId to null for all products in this category
                    foreach (var product in category.Products)
                    {
                        product.CategoryId = null;
                    }

                    // Remove the category
                    _context.Categories.Remove(category);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Category deleted successfully! {category.Products.Count} products were moved to 'No Category'.";
                }
                catch (DbUpdateException)
                {
                    TempData["ErrorMessage"] = "An error occurred while deleting the category.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Category not found.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }
    }
}