using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnowStoreWeb.Attributes;
using SnowStoreWeb.Models;

namespace SnowStoreWeb.Controllers.Admin
{
    [AuthorizeUser("Admin")]
    public class AdminPopupBannerController : Controller
    {
        private readonly SnowStoreContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminPopupBannerController(SnowStoreContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: PopupBanner
        public async Task<IActionResult> Index(string status = "all", int page = 1)
        {
            ViewData["CurrentStatusFilter"] = status;

            var banners = _context.PopupBanners.AsQueryable();

            // Filter by status
            switch (status)
            {
                case "active":
                    banners = banners.Where(b => b.Status == PopupStatus.Active);
                    break;
                case "inactive":
                    banners = banners.Where(b => b.Status == PopupStatus.Inactive);
                    break;
                    // "all" - no filter
            }

            // Pagination
            int pageSize = 10;
            var totalItems = await banners.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var result = await banners
                .OrderBy(b => b.DisplayOrder)
                .ThenByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(result);
        }

        // GET: PopupBanner/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var banner = await _context.PopupBanners
                .FirstOrDefaultAsync(m => m.Id == id);

            if (banner == null) return NotFound();

            return View(banner);
        }


        // GET: PopupBanner/Create
        public IActionResult Create()
        {
            var banner = new PopupBanner
            {
                Status = PopupStatus.Inactive // mặc định chọn Inactive
            };
            return View(banner);
        }



        // POST: PopupBanner/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Status")] PopupBanner banner, IFormFile imageFile)
        {
            ModelState.Remove("ImageUrl");
            ModelState.Remove("DisplayOrder");

            if (imageFile == null || imageFile.Length == 0)
            {
                ModelState.AddModelError("imageFile", "Vui lòng chọn hình ảnh để tải lên.");
            }

            if (ModelState.IsValid)
            {
                var imageUrl = await SaveImageAsync(imageFile);
                if (imageUrl == null)
                {
                    ModelState.AddModelError("imageFile", "Có lỗi khi tải lên hình ảnh. Vui lòng thử lại.");
                    return View(banner);
                }
                banner.ImageUrl = imageUrl;

                var maxDisplayOrder = await _context.PopupBanners.MaxAsync(b => (int?)b.DisplayOrder) ?? 0;
                banner.DisplayOrder = maxDisplayOrder + 1;

                banner.CreatedAt = DateTime.Now;
                banner.UpdatedAt = null;

                _context.Add(banner);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đã tạo thành công popup banner '{banner.Title}' với thứ tự {banner.DisplayOrder}";
                return RedirectToAction(nameof(Index));
            }

            return View(banner);
        }


        // GET: PopupBanner/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var banner = await _context.PopupBanners.FindAsync(id);
            if (banner == null) return NotFound();

            return View(banner);
        }

        // POST: PopupBanner/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,DisplayOrder,Status,CreatedAt,ImageUrl")] PopupBanner banner, IFormFile imageFile)
        {
            if (id != banner.Id) return NotFound();

            // Loại bỏ ImageUrl và imageFile khỏi kiểm tra ModelState
            ModelState.Remove("ImageUrl");
            ModelState.Remove("imageFile");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBanner = await _context.PopupBanners.FindAsync(id);
                    if (existingBanner == null) return NotFound();

                    var oldImageUrl = existingBanner.ImageUrl;

                    // Xử lý ảnh mới nếu có
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var newImageUrl = await SaveImageAsync(imageFile);
                        if (newImageUrl != null)
                        {
                            // Xóa ảnh cũ
                            if (!string.IsNullOrEmpty(oldImageUrl))
                            {
                                DeleteImageFile(oldImageUrl);
                            }
                            existingBanner.ImageUrl = newImageUrl;
                        }
                        else
                        {
                            ModelState.AddModelError("imageFile", "Có lỗi khi tải lên hình ảnh mới. Vui lòng thử lại.");
                            banner.ImageUrl = oldImageUrl;
                            return View(banner);
                        }
                    }
                    // Nếu không có ảnh mới, giữ nguyên ImageUrl hiện tại

                    existingBanner.Title = banner.Title;
                    existingBanner.Description = banner.Description;
                    existingBanner.DisplayOrder = banner.DisplayOrder;
                    existingBanner.Status = banner.Status;
                    existingBanner.UpdatedAt = DateTime.Now;

                    _context.Update(existingBanner);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Đã cập nhật thành công popup banner '{banner.Title}'";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BannerExists(banner.Id))
                        return NotFound();
                    throw;
                }
            }

            // Nếu xác thực thất bại, đảm bảo giữ ImageUrl hiện tại
            var currentBanner = await _context.PopupBanners.FindAsync(id);
            if (currentBanner != null)
            {
                banner.ImageUrl = currentBanner.ImageUrl;
            }

            return View(banner);
        }
        // POST: PopupBanner/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var banner = await _context.PopupBanners.FindAsync(id);
            if (banner == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy popup banner cần xóa";
                return RedirectToAction(nameof(Index));
            }

            // Delete associated image file
            if (!string.IsNullOrEmpty(banner.ImageUrl))
            {
                DeleteImageFile(banner.ImageUrl);
            }

            _context.PopupBanners.Remove(banner);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã xóa thành công popup banner '{banner.Title}'";
            return RedirectToAction(nameof(Index));
        }

        // POST: Toggle Status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var banner = await _context.PopupBanners.FindAsync(id);
            if (banner == null)
            {
                return Json(new { success = false, message = "Không tìm thấy popup banner" });
            }

            banner.Status = banner.Status == PopupStatus.Active ? PopupStatus.Inactive : PopupStatus.Active;
            banner.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                status = banner.Status,
                isActive = banner.Status == PopupStatus.Active,
                message = banner.Status == PopupStatus.Active ? "Đã kích hoạt hiển thị" : "Đã tắt hiển thị"
            });
        }

        // API: Get Active Banners for Frontend
        [HttpGet]
        public async Task<IActionResult> GetActiveBanners()
        {
            var activeBanners = await _context.PopupBanners
                .Where(b => b.Status == PopupStatus.Active)
                .OrderBy(b => b.DisplayOrder)
                .Select(b => new
                {
                    id = b.Id,
                    title = b.Title,
                    imageUrl = b.ImageUrl,
                    description = b.Description,
                    displayOrder = b.DisplayOrder
                })
                .ToListAsync();

            return Json(activeBanners);
        }

        private bool BannerExists(int id)
        {
            return _context.PopupBanners.Any(e => e.Id == id);
        }

        private async Task<string?> SaveImageAsync(IFormFile imageFile)
        {
            try
            {
                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    return null;
                }

                // Validate file size (max 5MB)
                if (imageFile.Length > 5 * 1024 * 1024)
                {
                    return null;
                }

                // Create uploads directory if it doesn't exist
                var uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "popup-banners");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsDir, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Return relative URL
                return $"/uploads/popup-banners/{fileName}";
            }
            catch
            {
                return null;
            }
        }

        private void DeleteImageFile(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl)) return;

                // Convert relative URL to physical path
                var relativePath = imageUrl.TrimStart('/');
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch
            {
                // Log error if needed, but don't throw
            }
        }
    }
}