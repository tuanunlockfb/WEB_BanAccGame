using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WEB_BanAccGame.Data;
using WEB_BanAccGame.Models;
using WEB_BanAccGame.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace WEB_BanAccGame.Controllers
{
    public class GameAccountsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 12;
        private readonly UserManager<ApplicationUser> _userManager;

        public GameAccountsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: GameAccounts
        public async Task<IActionResult> Index(string? category, string? searchString, string? sortOrder, string? currentFilter, int? page)
        {
            // Save current filter
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["PriceSortParm"] = sortOrder == "price" ? "price_desc" : "price";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var accounts = from a in _context.GameAccounts.Include(a => a.Category)
                          where a.IsAvailable
                          select a;

            // Filter by category
            if (!string.IsNullOrEmpty(category))
            {
                accounts = accounts.Where(a => a.Category!.Name == category);
            }

            // Filter by search string
            if (!string.IsNullOrEmpty(searchString))
            {
                accounts = accounts.Where(a => a.Title.Contains(searchString) 
                    || (a.Description != null && a.Description.Contains(searchString))
                    || (a.Server != null && a.Server.Contains(searchString)));
            }

            // Sort
            switch (sortOrder)
            {
                case "name_desc":
                    accounts = accounts.OrderByDescending(a => a.Title);
                    break;
                case "price":
                    accounts = accounts.OrderBy(a => a.Price);
                    break;
                case "price_desc":
                    accounts = accounts.OrderByDescending(a => a.Price);
                    break;
                default:
                    accounts = accounts.OrderBy(a => a.Title);
                    break;
            }

            int pageNumber = page ?? 1;
            var totalItems = await accounts.CountAsync();
            var paginatedAccounts = await PaginatedList<GameAccount>.CreateAsync(accounts.AsNoTracking(), pageNumber, PageSize);

            var categories = await _context.GameCategories.ToListAsync();

            var viewModel = new GameAccountListViewModel
            {
                GameAccounts = paginatedAccounts,
                Categories = new SelectList(categories, "Name", "Name", category),
                CurrentCategory = category,
                SearchString = searchString,
                CurrentFilter = searchString,
                CurrentSort = sortOrder,
                TotalItems = totalItems,
                PageNumber = pageNumber,
                TotalPages = paginatedAccounts.TotalPages
            };

            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = paginatedAccounts.TotalPages;

            return View(viewModel);
        }

        // GET: GameAccounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gameAccount = await _context.GameAccounts
                .Include(g => g.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (gameAccount == null)
            {
                return NotFound();
            }

            // Get related products
            var relatedProducts = await _context.GameAccounts
                .Include(g => g.Category)
                .Where(g => g.CategoryId == gameAccount.CategoryId && g.Id != gameAccount.Id && g.IsAvailable)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;

            return View(gameAccount);
        }

        // GET: GameAccounts/Category?name=FPS hoặc GameAccounts/Category/FPS
        [HttpGet]
        [Route("GameAccounts/Category/{name?}")]
        [Route("GameAccounts/Category")]
        public IActionResult Category(string? name)
        {
            // Kiểm tra nếu name là null từ route parameter, lấy từ query string
            if (string.IsNullOrEmpty(name))
            {
                name = Request.Query["name"];
            }
            
            if (string.IsNullOrEmpty(name))
            {
                return RedirectToAction(nameof(Index));
            }

            // Redirect về Index action với category parameter
            return RedirectToAction(nameof(Index), new { category = name });
        }

        // POST: GameAccounts/CompletePurchase
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompletePurchase(int id)
        {
            var gameAccount = await _context.GameAccounts
                .Include(g => g.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (gameAccount == null || !gameAccount.IsAvailable)
            {
                TempData["Error"] = "Tài khoản này không tồn tại hoặc đã được bán!";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);
            
            // Create order
            var order = new Order
            {
                OrderCode = $"ORD{DateTime.Now:yyyyMMddHHmmss}",
                UserId = user!.Id,
                TotalAmount = gameAccount.Price,
                Status = OrderStatus.Completed,
                OrderDate = DateTime.Now
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Create order detail
            var orderDetail = new OrderDetail
            {
                OrderId = order.Id,
                GameAccountId = gameAccount.Id,
                Quantity = 1,
                Price = gameAccount.Price
            };

            _context.OrderDetails.Add(orderDetail);

            // Mark account as sold
            gameAccount.IsAvailable = false;
            _context.Update(gameAccount);
            
            await _context.SaveChangesAsync();

            // Redirect to account info page
            return RedirectToAction(nameof(AccountInfo), new { orderId = order.Id });
        }

        // GET: GameAccounts/AccountInfo
        [Authorize]
        public async Task<IActionResult> AccountInfo(int orderId)
        {
            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.GameAccount)
                .ThenInclude(ga => ga.Category)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == user!.Id);

            if (order == null)
            {
                return NotFound();
            }

            var gameAccount = order.OrderDetails.FirstOrDefault()?.GameAccount;
            if (gameAccount == null)
            {
                return NotFound();
            }

            // Get password change URL based on game category
            ViewBag.PasswordChangeUrl = GetPasswordChangeUrl(gameAccount.Title, gameAccount.Category?.Name);

            return View(gameAccount);
        }

        private string GetPasswordChangeUrl(string? gameTitle, string? categoryName)
        {
            // Kiểm tra theo tên game trước
            var title = gameTitle?.ToLower() ?? "";
            
            if (title.Contains("valorant")) return "https://account.riotgames.com/";
            if (title.Contains("genshin")) return "https://account.mihoyo.com/";
            if (title.Contains("pubg")) return "https://my.games.app/";
            if (title.Contains("liên quân") || title.Contains("lien quan")) return "https://lienquan.garena.vn/";
            if (title.Contains("mobile legends")) return "https://account.mobilelegends.com/";
            if (title.Contains("league of legends") || title.Contains("lol")) return "https://account.riotgames.com/";
            if (title.Contains("free fire") || title.Contains("ff")) return "https://ff.garena.vn/";
            if (title.Contains("fortnite")) return "https://www.epicgames.com/account/password";
            if (title.Contains("minecraft")) return "https://account.mojang.com/password";
            
            // Nếu không tìm thấy, kiểm tra theo category
            return categoryName?.ToLower() switch
            {
                "mmorpg" => "https://account.playpark.com/",
                "fps" => "https://account.garena.com/",
                "moba" => "https://lienquan.garena.vn/",
                "battle royale" => "https://www.epicgames.com/account/password",
                "survival" => "https://account.mojang.com/password",
                _ => "#"
            };
        }
    }
} 