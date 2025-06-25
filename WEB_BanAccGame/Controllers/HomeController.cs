using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_BanAccGame.Data;
using WEB_BanAccGame.Models;
using WEB_BanAccGame.ViewModels;

namespace WEB_BanAccGame.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel
            {
                Categories = await _context.GameCategories.ToListAsync(),
                FeaturedAccounts = await _context.GameAccounts
                    .Include(g => g.Category)
                    .Where(g => g.IsAvailable)
                    .OrderByDescending(g => g.CreatedDate)
                    .Take(8)
                    .ToListAsync(),
                NewAccounts = await _context.GameAccounts
                    .Include(g => g.Category)
                    .Where(g => g.IsAvailable)
                    .OrderByDescending(g => g.CreatedDate)
                    .Take(4)
                    .ToListAsync()
            };

            return View(viewModel);
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
