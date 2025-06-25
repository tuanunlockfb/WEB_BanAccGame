using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_BanAccGame.Data;
using WEB_BanAccGame.Services;
using WEB_BanAccGame.ViewModels;

namespace WEB_BanAccGame.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ShoppingCart _shoppingCart;
        private readonly ILogger<ShoppingCartController> _logger;

        public ShoppingCartController(ApplicationDbContext context, ShoppingCart shoppingCart, ILogger<ShoppingCartController> logger)
        {
            _context = context;
            _shoppingCart = shoppingCart;
            _logger = logger;
        }

        // GET: ShoppingCart
        public async Task<IActionResult> Index()
        {
            var items = await _shoppingCart.GetShoppingCartItemsAsync();
            _shoppingCart.ShoppingCartItems = items;

            var viewModel = new ShoppingCartViewModel
            {
                ShoppingCart = _shoppingCart,
                ShoppingCartTotal = await _shoppingCart.GetShoppingCartTotalAsync(),
                ItemCount = await _shoppingCart.GetCartItemCountAsync()
            };

            return View(viewModel);
        }

        // POST: ShoppingCart/AddToCart/5
        [HttpPost]
        public async Task<IActionResult> AddToCart(int id, string? returnUrl = null)
        {
            try
            {
                var gameAccount = await _context.GameAccounts
                    .Include(g => g.Category)
                    .FirstOrDefaultAsync(g => g.Id == id);

                if (gameAccount == null)
                {
                    TempData["Error"] = "Sản phẩm không tồn tại!";
                    return RedirectToAction(nameof(Index));
                }

                if (!gameAccount.IsAvailable)
                {
                    TempData["Error"] = "Sản phẩm đã được bán!";
                    return RedirectToAction("Details", "GameAccounts", new { id });
                }

                await _shoppingCart.AddToCartAsync(gameAccount);
                
                TempData["Success"] = $"Đã thêm {gameAccount.Title} vào giỏ hàng!";
                _logger.LogInformation($"Added {gameAccount.Title} to cart. Cart ID: {_shoppingCart.ShoppingCartId}");

                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart");
                TempData["Error"] = "Có lỗi xảy ra khi thêm vào giỏ hàng!";
                return RedirectToAction("Details", "GameAccounts", new { id });
            }
        }

        // POST: ShoppingCart/RemoveFromCart/5
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var gameAccount = await _context.GameAccounts
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gameAccount != null)
            {
                await _shoppingCart.RemoveFromCartAsync(gameAccount);
                TempData["Success"] = "Đã xóa sản phẩm khỏi giỏ hàng!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: ShoppingCart/ClearCart
        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            await _shoppingCart.ClearCartAsync();
            TempData["Success"] = "Đã xóa toàn bộ giỏ hàng!";
            return RedirectToAction(nameof(Index));
        }

        // GET: ShoppingCart/GetCartItemCount
        public async Task<IActionResult> GetCartItemCount()
        {
            var count = await _shoppingCart.GetCartItemCountAsync();
            return Json(new { count });
        }

        // GET: ShoppingCart/Debug
        [HttpGet]
        public async Task<IActionResult> Debug()
        {
            var allItems = await _context.ShoppingCartItems
                .Include(s => s.GameAccount)
                .ToListAsync();
                
            var debugInfo = new
            {
                CurrentCartId = _shoppingCart.ShoppingCartId,
                CurrentCartItemsCount = await _shoppingCart.GetCartItemCountAsync(),
                AllCartItemsInDb = allItems.Select(i => new
                {
                    i.Id,
                    i.ShoppingCartId,
                    GameAccountId = i.GameAccountId,
                    GameAccountTitle = i.GameAccount?.Title,
                    i.Quantity
                })
            };
            
            return Json(debugInfo);
        }
    }
} 