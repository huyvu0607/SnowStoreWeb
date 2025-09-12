using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SnowStoreWeb.Attributes;
using SnowStoreWeb.Models;
using Microsoft.AspNetCore.Http;

namespace SnowStoreWeb.Controllers_Admin
{
    [AuthorizeUser("Admin")]
    public class AdminProductsController : Controller
    {
        private readonly SnowStoreContext _context;
        private readonly string _imageUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");

        public AdminProductsController(SnowStoreContext context)
        {
            _context = context;
            if (!Directory.Exists(_imageUploadPath))
            {
                Directory.CreateDirectory(_imageUploadPath);
            }
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: AdminProducts
        public async Task<IActionResult> Index()
        {
            var snowStoreContext = _context.Products.Include(p => p.Category);
            return View(await snowStoreContext.ToListAsync());
        }

        // GET: AdminProducts/Details/5
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

            return View(product);
        }

        // GET: AdminProducts/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }

        // POST: AdminProducts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,Name,Description,Price,StockQuantity,IsHot,IsBestSeller")] Product product, IFormFile MainImageFile, IFormFile[] AdditionalImageFiles)
        {
            if (ModelState.IsValid)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                // Xử lý ảnh chính
                if (MainImageFile != null && MainImageFile.Length > 0)
                {
                    if (MainImageFile.Length > 5 * 1024 * 1024) // 5MB
                    {
                        ModelState.AddModelError("MainImageFile", "Ảnh chính không được lớn hơn 5MB.");
                        ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
                        return View(product);
                    }

                    var extension = Path.GetExtension(MainImageFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("MainImageFile", "Chỉ chấp nhận các định dạng ảnh: .jpg, .jpeg, .png, .gif");
                        ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
                        return View(product);
                    }

                    var fileName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(_imageUploadPath, fileName);

                    try
                    {
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await MainImageFile.CopyToAsync(stream);
                        }
                        product.ImageUrl = $"/images/products/{fileName}";
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Lỗi khi lưu ảnh chính: {ex.Message}");
                        ModelState.AddModelError("MainImageFile", "Không thể lưu ảnh chính.");
                        ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
                        return View(product);
                    }
                }

                product.CreatedDate = DateTime.Now;
                _context.Add(product);
                try
                {
                    await _context.SaveChangesAsync(); // Lưu product để lấy ProductId
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Lỗi khi lưu sản phẩm: {ex.Message}");
                    ModelState.AddModelError("", "Lỗi khi lưu sản phẩm.");
                    ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
                    return View(product);
                }

                // Xử lý ảnh phụ
                if (AdditionalImageFiles != null && AdditionalImageFiles.Length > 0)
                {
                    if (AdditionalImageFiles.Length > 5)
                    {
                        ModelState.AddModelError("AdditionalImageFiles", "Chỉ được tải lên tối đa 5 ảnh phụ.");
                        ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
                        return View(product);
                    }

                    int sortOrder = 1;
                    foreach (var imageFile in AdditionalImageFiles)
                    {
                        if (imageFile.Length > 0)
                        {
                            if (imageFile.Length > 5 * 1024 * 1024) // 5MB
                            {
                                ModelState.AddModelError("AdditionalImageFiles", "Ảnh phụ không được lớn hơn 5MB.");
                                ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
                                return View(product);
                            }

                            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                            if (!allowedExtensions.Contains(extension))
                            {
                                ModelState.AddModelError("AdditionalImageFiles", "Chỉ chấp nhận các định dạng ảnh: .jpg, .jpeg, .png, .gif");
                                ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
                                return View(product);
                            }

                            var fileName = Guid.NewGuid().ToString() + extension;
                            var filePath = Path.Combine(_imageUploadPath, fileName);

                            try
                            {
                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await imageFile.CopyToAsync(stream);
                                }

                                var productImage = new ProductImage
                                {
                                    ProductId = product.ProductId,
                                    ImageUrl = $"/images/products/{fileName}",
                                    SortOrder = sortOrder++
                                };
                                _context.ProductImages.Add(productImage);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Lỗi khi lưu ảnh phụ: {ex.Message}");
                                ModelState.AddModelError("AdditionalImageFiles", "Không thể lưu ảnh phụ.");
                                ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
                                return View(product);
                            }
                        }
                    }
                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Lỗi khi lưu ảnh phụ: {ex.Message}");
                        ModelState.AddModelError("", "Lỗi khi lưu ảnh phụ.");
                        ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
                        return View(product);
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // GET: AdminProducts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.ProductImages.OrderBy(pi => pi.SortOrder))
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // POST: AdminProducts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile MainImageFile, IFormFile[] AdditionalImageFiles, int[] DeletedImageIds)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            try
            {
                // Lấy sản phẩm hiện tại từ DB
                var existingProduct = await _context.Products
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (existingProduct == null)
                {
                    return NotFound();
                }

                // Xử lý xóa ảnh phụ được đánh dấu
                if (DeletedImageIds != null && DeletedImageIds.Length > 0)
                {
                    var imagesToDelete = existingProduct.ProductImages
                        .Where(pi => DeletedImageIds.Contains(pi.ImageId))
                        .ToList();

                    foreach (var imageToDelete in imagesToDelete)
                    {
                        // Xóa file ảnh
                        if (!string.IsNullOrEmpty(imageToDelete.ImageUrl))
                        {
                            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageToDelete.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(imagePath))
                            {
                                try
                                {
                                    System.IO.File.Delete(imagePath);
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Không thể xóa file: {ex.Message}");
                                }
                            }
                        }
                        _context.ProductImages.Remove(imageToDelete);
                    }
                }

                // Cập nhật thông tin sản phẩm
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.StockQuantity = product.StockQuantity;
                existingProduct.IsHot = product.IsHot;
                existingProduct.IsBestSeller = product.IsBestSeller;

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                // Xử lý ảnh chính mới
                if (MainImageFile != null && MainImageFile.Length > 0)
                {
                    // Validate ảnh chính
                    if (MainImageFile.Length > 5 * 1024 * 1024)
                    {
                        TempData["ErrorMessage"] = "Ảnh chính không được lớn hơn 5MB.";
                        ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
                        return View(existingProduct);
                    }

                    var extension = Path.GetExtension(MainImageFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        TempData["ErrorMessage"] = "Chỉ chấp nhận các định dạng ảnh: .jpg, .jpeg, .png, .gif";
                        ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
                        return View(existingProduct);
                    }

                    // Xóa ảnh chính cũ
                    if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingProduct.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            try
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Không thể xóa ảnh cũ: {ex.Message}");
                            }
                        }
                    }

                    // Lưu ảnh chính mới
                    var fileName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(_imageUploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await MainImageFile.CopyToAsync(stream);
                    }
                    existingProduct.ImageUrl = $"/images/products/{fileName}";
                }

                // Xử lý ảnh phụ mới
                if (AdditionalImageFiles != null && AdditionalImageFiles.Length > 0)
                {
                    var currentImageCount = existingProduct.ProductImages.Count;
                    if (currentImageCount + AdditionalImageFiles.Length > 5)
                    {
                        TempData["ErrorMessage"] = $"Tổng số ảnh phụ không được vượt quá 5. Hiện tại: {currentImageCount}";
                        ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
                        return View(existingProduct);
                    }

                    var maxSortOrder = existingProduct.ProductImages.Any()
                        ? existingProduct.ProductImages.Max(pi => pi.SortOrder)
                        : 0;

                    foreach (var imageFile in AdditionalImageFiles)
                    {
                        if (imageFile.Length > 0)
                        {
                            // Validate ảnh phụ
                            if (imageFile.Length > 5 * 1024 * 1024)
                            {
                                TempData["ErrorMessage"] = "Ảnh phụ không được lớn hơn 5MB.";
                                ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
                                return View(existingProduct);
                            }

                            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                            if (!allowedExtensions.Contains(extension))
                            {
                                TempData["ErrorMessage"] = "Chỉ chấp nhận các định dạng ảnh: .jpg, .jpeg, .png, .gif";
                                ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
                                return View(existingProduct);
                            }

                            // Lưu ảnh phụ
                            var fileName = Guid.NewGuid().ToString() + extension;
                            var filePath = Path.Combine(_imageUploadPath, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await imageFile.CopyToAsync(stream);
                            }

                            var productImage = new ProductImage
                            {
                                ProductId = existingProduct.ProductId,
                                ImageUrl = $"/images/products/{fileName}",
                                SortOrder = ++maxSortOrder
                            };
                            _context.ProductImages.Add(productImage);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi cập nhật sản phẩm: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật sản phẩm.";
                ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
                return View(product);
            }
        }

        // POST: AdminProducts/DeleteImage/5 - ĐÃ SỬA LỖI XÓA FILE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== DELETEIMAGE ACTION - ImageId: {imageId} ===");

                if (imageId <= 0)
                {
                    System.Diagnostics.Debug.WriteLine($"ImageId không hợp lệ: {imageId}");
                    return Json(new { success = false, message = "ID ảnh không hợp lệ" });
                }

                var productImage = await _context.ProductImages.FindAsync(imageId);
                if (productImage == null)
                {
                    System.Diagnostics.Debug.WriteLine($"KHÔNG TÌM THẤY ẢNH: ImageId = {imageId}");
                    return Json(new { success = false, message = "Không tìm thấy ảnh" });
                }

                System.Diagnostics.Debug.WriteLine($"Tìm thấy ảnh: {productImage.ImageUrl}, ProductId: {productImage.ProductId}");

                // Xóa tệp ảnh trước khi xóa record
                bool fileDeleted = false;
                if (!string.IsNullOrEmpty(productImage.ImageUrl))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", productImage.ImageUrl.TrimStart('/'));
                    System.Diagnostics.Debug.WriteLine($"Đường dẫn file: {imagePath}");

                    if (System.IO.File.Exists(imagePath))
                    {
                        try
                        {
                            System.IO.File.Delete(imagePath);
                            fileDeleted = true;
                            System.Diagnostics.Debug.WriteLine($"ĐÃ XÓA FILE: {imagePath}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"LỖI KHI XÓA FILE: {ex.Message}");
                            // Vẫn tiếp tục xóa record trong DB
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"FILE KHÔNG TỒN TẠI: {imagePath}");
                    }
                }

                // Xóa record trong database
                System.Diagnostics.Debug.WriteLine("Đang xóa record trong database...");
                _context.ProductImages.Remove(productImage);
                await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine("ĐÃ XÓA RECORD TRONG DATABASE");

                return Json(new
                {
                    success = true,
                    message = "Xóa ảnh thành công",
                    fileDeleted = fileDeleted,
                    imageId = imageId
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LỖI NGOẠI LỆ: {ex.Message}");
                return Json(new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }

        // POST: AdminProducts/UpdateImageOrder
        [HttpPost]
        public async Task<IActionResult> UpdateImageOrder([FromBody] UpdateImageOrderModel model)
        {
            try
            {
                if (model?.ImageIds == null || !model.ImageIds.Any())
                {
                    return BadRequest(new { success = false, message = "Danh sách ImageIds không hợp lệ." });
                }

                var images = await _context.ProductImages
                    .Where(pi => model.ImageIds.Contains(pi.ImageId))
                    .ToListAsync();

                for (int i = 0; i < model.ImageIds.Length; i++)
                {
                    var image = images.FirstOrDefault(img => img.ImageId == model.ImageIds[i]);
                    if (image != null)
                    {
                        image.SortOrder = i + 1;
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Cập nhật thứ tự thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật thứ tự ảnh: " + ex.Message });
            }
        }



        // POST: AdminProducts/SetMainImage
        [HttpPost]
        public async Task<IActionResult> SetMainImage([FromBody] SetMainImageModel model)
        {
            try
            {
                if (model.ProductId <= 0 || model.ImageId <= 0 || string.IsNullOrEmpty(model.ImageUrl))
                {
                    return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
                }

                var product = await _context.Products.FindAsync(model.ProductId);
                if (product == null)
                {
                    return NotFound(new { success = false, message = "Sản phẩm không tồn tại." });
                }

                var productImage = await _context.ProductImages.FindAsync(model.ImageId);
                if (productImage == null || productImage.ProductId != model.ProductId)
                {
                    return BadRequest(new { success = false, message = "Ảnh không hợp lệ." });
                }

                // Xóa ảnh chính cũ nếu có
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        try
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Không thể xóa ảnh cũ: {ex.Message}");
                        }
                    }
                }

                // Đặt ảnh mới làm ảnh chính và xóa khỏi danh sách ảnh phụ
                product.ImageUrl = model.ImageUrl;
                _context.ProductImages.Remove(productImage);

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Đặt ảnh chính thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi đặt ảnh chính: " + ex.Message });
            }
        }

        // GET: AdminProducts/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

            return View(product);
        }

        // POST: AdminProducts/Delete/5 - ĐÃ SỬA LỖI XÓA TẤT CẢ ẢNH
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== BẮT ĐẦU XÓA SẢN PHẨM ID: {id} ===");

                var product = await _context.Products
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (product == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Không tìm thấy sản phẩm ID: {id}");
                    return NotFound();
                }

                System.Diagnostics.Debug.WriteLine($"Tìm thấy sản phẩm: {product.Name}");
                System.Diagnostics.Debug.WriteLine($"Số lượng ảnh phụ: {product.ProductImages?.Count ?? 0}");

                // Danh sách tất cả đường dẫn ảnh cần xóa
                var imagePaths = new List<string>();

                // Thêm ảnh chính vào danh sách xóa
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    imagePaths.Add(product.ImageUrl);
                    System.Diagnostics.Debug.WriteLine($"Thêm ảnh chính vào danh sách xóa: {product.ImageUrl}");
                }

                // Thêm tất cả ảnh phụ vào danh sách xóa
                if (product.ProductImages != null && product.ProductImages.Any())
                {
                    foreach (var image in product.ProductImages)
                    {
                        if (!string.IsNullOrEmpty(image.ImageUrl))
                        {
                            imagePaths.Add(image.ImageUrl);
                            System.Diagnostics.Debug.WriteLine($"Thêm ảnh phụ vào danh sách xóa: {image.ImageUrl}");
                        }
                    }
                }

                // Xóa tất cả file ảnh
                foreach (var imageUrl in imagePaths)
                {
                    try
                    {
                        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                            System.Diagnostics.Debug.WriteLine($"✓ Đã xóa file: {imagePath}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"⚠ File không tồn tại: {imagePath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"✗ Lỗi khi xóa file {imageUrl}: {ex.Message}");
                        // Tiếp tục xóa các file khác
                    }
                }

                // Xóa tất cả ảnh phụ trong database (CASCADE sẽ tự động xóa)
                if (product.ProductImages != null && product.ProductImages.Any())
                {
                    _context.ProductImages.RemoveRange(product.ProductImages);
                    System.Diagnostics.Debug.WriteLine($"Đánh dấu xóa {product.ProductImages.Count} ảnh phụ trong database");
                }

                // Xóa sản phẩm
                _context.Products.Remove(product);
                System.Diagnostics.Debug.WriteLine("Đánh dấu xóa sản phẩm trong database");

                // Lưu thay đổi
                await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine("✓ ĐÃ LỮU THAY ĐỔI - XÓA SẢN PHẨM THÀNH CÔNG");

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ LỖI KHI XÓA SẢN PHẨM: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Chi tiết lỗi: {ex.InnerException?.Message}");

                // Trả về trang lỗi hoặc thông báo lỗi
                TempData["ErrorMessage"] = "Không thể xóa sản phẩm: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // Helper method để xóa file ảnh an toàn
        private bool DeleteImageFile(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return false;

            try
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                    System.Diagnostics.Debug.WriteLine($"Đã xóa file: {imagePath}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi xóa file {imageUrl}: {ex.Message}");
            }
            return false;
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }

    // Model để nhận dữ liệu từ AJAX khi cập nhật thứ tự
    public class UpdateImageOrderModel
    {
        public int[] ImageIds { get; set; }
    }

    // Model để nhận dữ liệu từ AJAX khi đặt ảnh chính
    public class SetMainImageModel
    {
        public int ProductId { get; set; }
        public int ImageId { get; set; }
        public string ImageUrl { get; set; }
    }
}