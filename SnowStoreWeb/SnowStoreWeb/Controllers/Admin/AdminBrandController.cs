using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnowStoreWeb.Attributes;
using SnowStoreWeb.Models;

namespace SnowStoreWeb.Controllers.Admin
{
    [AuthorizeUser("Admin")]
    public class AdminBrandController : Controller
    {
        private readonly SnowStoreContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminBrandController(SnowStoreContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Brand
        public async Task<IActionResult> Index()
        {
            return View(await _context.Brands.ToListAsync());
        }

        // GET: Brand/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brand = await _context.Brands
                .FirstOrDefaultAsync(m => m.BrandId == id);
            if (brand == null)
            {
                return NotFound();
            }

            return View(brand);
        }

        // GET: Brand/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Brand/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BrandId,Name,LogoUrl")] Brand brand, IFormFile? logoFile)
        {
            try
            {
                // Handle file upload
                if (logoFile != null && logoFile.Length > 0)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(logoFile.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("logoFile", "Chỉ hỗ trợ file ảnh định dạng JPG, PNG, GIF.");
                        return View(brand);
                    }

                    // Validate file size (5MB)
                    if (logoFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("logoFile", "Kích thước file không được vượt quá 5MB.");
                        return View(brand);
                    }

                    // Generate unique filename
                    var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "brands");

                    // Create directory if it doesn't exist
                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }

                    var filePath = Path.Combine(uploadsPath, uniqueFileName);

                    // Save file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await logoFile.CopyToAsync(stream);
                    }

                    // Set the LogoUrl to the relative path
                    brand.LogoUrl = $"/uploads/brands/{uniqueFileName}";
                }

                if (ModelState.IsValid)
                {
                    _context.Add(brand);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Thương hiệu đã được tạo thành công!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Có lỗi xảy ra khi tạo thương hiệu: {ex.Message}");
            }

            return View(brand);
        }

        // GET: Brand/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            return View(brand);
        }

        // POST: Brand/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BrandId,Name,LogoUrl")] Brand brand, IFormFile? logoFile)
        {
            if (id != brand.BrandId)
            {
                return NotFound();
            }

            try
            {
                var existingBrand = await _context.Brands.AsNoTracking().FirstOrDefaultAsync(b => b.BrandId == id);
                if (existingBrand == null)
                {
                    return NotFound();
                }

                // Handle file upload
                if (logoFile != null && logoFile.Length > 0)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExtension = Path.GetExtension(logoFile.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("logoFile", "Chỉ hỗ trợ file ảnh định dạng JPG, PNG, GIF.");
                        return View(brand);
                    }

                    // Validate file size (5MB)
                    if (logoFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("logoFile", "Kích thước file không được vượt quá 5MB.");
                        return View(brand);
                    }

                    // Delete old file if it exists and is a local file
                    if (!string.IsNullOrEmpty(existingBrand.LogoUrl) && existingBrand.LogoUrl.StartsWith("/uploads/"))
                    {
                        var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingBrand.LogoUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Generate unique filename
                    var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "brands");

                    // Create directory if it doesn't exist
                    if (!Directory.Exists(uploadsPath))
                    {
                        Directory.CreateDirectory(uploadsPath);
                    }

                    var filePath = Path.Combine(uploadsPath, uniqueFileName);

                    // Save file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await logoFile.CopyToAsync(stream);
                    }

                    // Set the LogoUrl to the relative path
                    brand.LogoUrl = $"/uploads/brands/{uniqueFileName}";
                }
                else
                {
                    // Keep existing logo if no new file is uploaded
                    brand.LogoUrl = existingBrand.LogoUrl;
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(brand);
                        await _context.SaveChangesAsync();

                        TempData["SuccessMessage"] = "Thương hiệu đã được cập nhật thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!BrandExists(brand.BrandId))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Có lỗi xảy ra khi cập nhật thương hiệu: {ex.Message}");
            }

            return View(brand);
        }

        // GET: Brand/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brand = await _context.Brands
                .FirstOrDefaultAsync(m => m.BrandId == id);
            if (brand == null)
            {
                return NotFound();
            }

            return View(brand);
        }

        // POST: Brand/Delete/5
        // POST: Brand/Delete/5 (Alternative version using ExecuteUpdateAsync - EF Core 7+)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand != null)
            {
                try
                {
                    // Set BrandId to null for all products that belong to this brand (EF Core 7+ only)
                    await _context.Products
                        .Where(p => p.BrandId == id)
                        .ExecuteUpdateAsync(p => p.SetProperty(x => x.BrandId, (int?)null));

                    // Delete associated logo file if it's a local file
                    if (!string.IsNullOrEmpty(brand.LogoUrl) && brand.LogoUrl.StartsWith("/uploads/"))
                    {
                        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, brand.LogoUrl.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            try
                            {
                                System.IO.File.Delete(filePath);
                            }
                            catch (Exception ex)
                            {
                                // Log the error but don't stop the deletion process
                                Console.WriteLine($"Error deleting file: {ex.Message}");
                            }
                        }
                    }

                    // Now delete the brand
                    _context.Brands.Remove(brand);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Thương hiệu đã được xóa thành công! Tất cả sản phẩm thuộc thương hiệu này đã được cập nhật.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Có lỗi xảy ra khi xóa thương hiệu: {ex.Message}";
                    return RedirectToAction(nameof(Index));
                }
            }

            return RedirectToAction(nameof(Index));
        }
        // Thêm method này vào AdminBrandController.cs

        // POST: Brand/BulkDelete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete([FromBody] List<int> brandIds)
        {
            try
            {
                if (brandIds == null || brandIds.Count == 0)
                {
                    return Json(new { success = false, message = "Không có thương hiệu nào được chọn." });
                }

                var brands = await _context.Brands
                    .Where(b => brandIds.Contains(b.BrandId))
                    .ToListAsync();

                if (brands.Count == 0)
                {
                    return Json(new { success = false, message = "Không tìm thấy thương hiệu nào để xóa." });
                }

                int deletedCount = 0;
                var errors = new List<string>();

                foreach (var brand in brands)
                {
                    try
                    {
                        // Set BrandId to null for all products that belong to this brand
                        await _context.Products
                            .Where(p => p.BrandId == brand.BrandId)
                            .ExecuteUpdateAsync(p => p.SetProperty(x => x.BrandId, (int?)null));

                        // Delete associated logo file if it's a local file
                        if (!string.IsNullOrEmpty(brand.LogoUrl) && brand.LogoUrl.StartsWith("/uploads/"))
                        {
                            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, brand.LogoUrl.TrimStart('/'));
                            if (System.IO.File.Exists(filePath))
                            {
                                try
                                {
                                    System.IO.File.Delete(filePath);
                                }
                                catch (Exception ex)
                                {
                                    // Log the error but don't stop the deletion process
                                    Console.WriteLine($"Error deleting file for brand {brand.Name}: {ex.Message}");
                                }
                            }
                        }

                        // Remove the brand
                        _context.Brands.Remove(brand);
                        deletedCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Lỗi khi xóa thương hiệu '{brand.Name}': {ex.Message}");
                    }
                }

                // Save all changes
                await _context.SaveChangesAsync();

                if (errors.Count > 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Đã xóa {deletedCount} thương hiệu. Có {errors.Count} lỗi xảy ra.",
                        errors = errors
                    });
                }

                return Json(new
                {
                    success = true,
                    message = $"Đã xóa thành công {deletedCount} thương hiệu!"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Có lỗi xảy ra: {ex.Message}"
                });
            }
        }
        private bool BrandExists(int id)
        {
            return _context.Brands.Any(e => e.BrandId == id);
        }

        // Helper method to delete file
        private void DeleteFile(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    System.IO.File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    // Log error but don't throw
                    Console.WriteLine($"Error deleting file {filePath}: {ex.Message}");
                }
            }
        }
    }
}