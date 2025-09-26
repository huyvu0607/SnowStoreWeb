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

            // Thêm dòng này để debug
            System.Diagnostics.Debug.WriteLine($"Active banners count: {activeBanners.Count}");
            foreach (var banner in activeBanners)
            {
                System.Diagnostics.Debug.WriteLine($"Banner: {banner.Title} - {banner.ImageUrl}");
            }

            ViewBag.ActiveBanners = activeBanners;
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