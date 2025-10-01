using Microsoft.AspNetCore.Mvc;
using SnowStoreWeb.Models;
using System.Diagnostics;

namespace SnowStoreWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SnowStoreContext _dbContext;

        // CHỈ có 1 constructor duy nhất
        public HomeController(ILogger<HomeController> logger, SnowStoreContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var activeBanners = _dbContext.PopupBanners
                                         .Where(b => b.Status == PopupStatus.Active)
                                         .OrderBy(b => b.DisplayOrder)
                                         .ToList();

            var messages = _dbContext.Messages
                            .OrderBy(m => m.Order) // sắp xếp theo thứ tự bạn định nghĩa
                            .ToList();

            // Thêm dòng này để debug
            System.Diagnostics.Debug.WriteLine($"Active banners count: {activeBanners.Count}");
            foreach (var banner in activeBanners)
            {
                System.Diagnostics.Debug.WriteLine($"Banner: {banner.Title} - {banner.ImageUrl}");
            }

            System.Diagnostics.Debug.WriteLine($"Messages count: {messages.Count}");
            foreach (var msg in messages)
            {
                System.Diagnostics.Debug.WriteLine($"Message: {msg.Description} - {msg.ImageUrl}");
            }

            ViewBag.ActiveBanners = activeBanners;
            ViewBag.Messages = messages;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}