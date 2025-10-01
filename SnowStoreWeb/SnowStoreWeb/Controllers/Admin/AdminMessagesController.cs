using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnowStoreWeb.Attributes;
using SnowStoreWeb.Models;

namespace SnowStoreWeb.Controllers
{
    [AuthorizeUser("Admin")]
    public class AdminMessagesController : Controller
    {
        private readonly SnowStoreContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminMessagesController(SnowStoreContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Message
        public async Task<IActionResult> Index(string searchTerm, string sortOrder)
        {
            ViewData["CurrentFilter"] = searchTerm;
            ViewData["OrderSortParm"] = String.IsNullOrEmpty(sortOrder) ? "order_desc" : "";
            ViewData["DescSortParm"] = sortOrder == "Description" ? "desc_desc" : "Description";

            var messages = from m in _context.Messages
                           select m;

            if (!String.IsNullOrEmpty(searchTerm))
            {
                messages = messages.Where(m => m.Description.Contains(searchTerm));
            }

            switch (sortOrder)
            {
                case "order_desc":
                    messages = messages.OrderByDescending(m => m.Order);
                    break;
                case "Description":
                    messages = messages.OrderBy(m => m.Description);
                    break;
                case "desc_desc":
                    messages = messages.OrderByDescending(m => m.Description);
                    break;
                default:
                    messages = messages.OrderBy(m => m.Order);
                    break;
            }

            return View(await messages.ToListAsync());
        }

        // GET: API endpoint to get next order number
        [HttpGet]
        public async Task<IActionResult> GetNextOrder()
        {
            try
            {
                var maxOrder = await _context.Messages
                    .Select(m => (int?)m.Order)
                    .MaxAsync();

                int nextOrder = (maxOrder ?? -1) + 1;

                return Json(new { nextOrder = nextOrder });
            }
            catch
            {
                return Json(new { nextOrder = 0 });
            }
        }

        // GET: Message/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == id);
            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        // GET: Message/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Message/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,Order")] Message message, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                // Validate that image file is provided
                if (imageFile == null || imageFile.Length == 0)
                {
                    ModelState.AddModelError("imageFile", "Please select an image file.");
                    return View(message);
                }

                // Handle image upload
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "Message");
                Directory.CreateDirectory(uploadsFolder); // Create directory if it doesn't exist

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                message.ImageUrl = "/images/Message/" + uniqueFileName;

                _context.Add(message);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Message created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(message);
        }

        // GET: Message/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }
            return View(message);
        }

        // POST: Message/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description,Order,ImageUrl")] Message message, IFormFile imageFile)
        {
            if (id != message.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingMessage = await _context.Messages.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);

                    // Handle image upload
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(existingMessage.ImageUrl))
                        {
                            string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, existingMessage.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Save new image
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "Message");
                        Directory.CreateDirectory(uploadsFolder);

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        message.ImageUrl = "/images/Message/" + uniqueFileName;
                    }
                    else
                    {
                        // Keep existing image URL if no new image uploaded
                        message.ImageUrl = existingMessage.ImageUrl;
                    }

                    _context.Update(message);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Message updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MessageExists(message.Id))
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
            return View(message);
        }

        // POST: Message/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message != null)
            {
                // Delete associated image file
                if (!string.IsNullOrEmpty(message.ImageUrl))
                {
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, message.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Message deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Message/DeleteMultiple
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMultiple(int[] selectedIds)
        {
            if (selectedIds != null && selectedIds.Length > 0)
            {
                var messagesToDelete = await _context.Messages
                    .Where(m => selectedIds.Contains(m.Id))
                    .ToListAsync();

                foreach (var message in messagesToDelete)
                {
                    // Delete associated image files
                    if (!string.IsNullOrEmpty(message.ImageUrl))
                    {
                        string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, message.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                }

                _context.Messages.RemoveRange(messagesToDelete);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Successfully deleted {messagesToDelete.Count} message(s)!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool MessageExists(int id)
        {
            return _context.Messages.Any(e => e.Id == id);
        }
        // POST: AdminMessages/UpdateOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrder([FromBody] List<MessageOrderUpdate> orderData)
        {
            if (orderData == null || !orderData.Any())
            {
                return BadRequest("No order data provided");
            }

            try
            {
                foreach (var item in orderData)
                {
                    var message = await _context.Messages.FindAsync(item.Id);
                    if (message != null)
                    {
                        message.Order = item.Order;
                        _context.Update(message);
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Order updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating order: " + ex.Message });
            }
        }
    }
    public class MessageOrderUpdate
    {
        public int Id { get; set; }
        public int Order { get; set; }
    }
}