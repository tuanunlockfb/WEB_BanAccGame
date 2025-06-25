using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_BanAccGame.Data;
using WEB_BanAccGame.Models;
using WEB_BanAccGame.Services;
using WEB_BanAccGame.ViewModels;

namespace WEB_BanAccGame.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ShoppingCart _shoppingCart;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationDbContext context, ShoppingCart shoppingCart, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _shoppingCart = shoppingCart;
            _userManager = userManager;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.GameAccount)
                .Where(o => o.UserId == user!.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.GameAccount)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user!.Id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Checkout
        public async Task<IActionResult> Checkout()
        {
            var items = await _shoppingCart.GetShoppingCartItemsAsync();
            if (!items.Any())
            {
                return RedirectToAction("Index", "ShoppingCart");
            }

            var user = await _userManager.GetUserAsync(User);
            var viewModel = new CheckoutViewModel
            {
                CartItems = items,
                TotalAmount = await _shoppingCart.GetShoppingCartTotalAsync(),
                FullName = user?.FullName ?? string.Empty,
                Email = user?.Email ?? string.Empty,
                PhoneNumber = user?.PhoneNumber ?? string.Empty
            };

            return View(viewModel);
        }

        // POST: Orders/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var items = await _shoppingCart.GetShoppingCartItemsAsync();
            if (!items.Any())
            {
                return RedirectToAction("Index", "ShoppingCart");
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var order = new Order
                {
                    OrderCode = $"ORD{DateTime.Now:yyyyMMddHHmmss}",
                    UserId = user!.Id,
                    TotalAmount = await _shoppingCart.GetShoppingCartTotalAsync(),
                    Notes = model.Notes,
                    Status = OrderStatus.Pending
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var item in items)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.Id,
                        GameAccountId = item.GameAccountId,
                        Quantity = item.Quantity,
                        Price = item.GameAccount!.Price
                    };
                    _context.OrderDetails.Add(orderDetail);

                    // Mark game account as sold
                    item.GameAccount.IsAvailable = false;
                }

                await _context.SaveChangesAsync();
                await _shoppingCart.ClearCartAsync();

                return RedirectToAction(nameof(OrderConfirmation), new { id = order.Id });
            }

            model.CartItems = items;
            model.TotalAmount = await _shoppingCart.GetShoppingCartTotalAsync();
            return View(model);
        }

        // GET: Orders/OrderConfirmation/5
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.GameAccount)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user!.Id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
} 