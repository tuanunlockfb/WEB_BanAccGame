using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_BanAccGame.Data;
using WEB_BanAccGame.Models;

namespace WEB_BanAccGame.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Wishlist
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var wishlistItems = await _context.Wishlists
                .Include(w => w.GameAccount)
                .ThenInclude(g => g!.Category)
                .Where(w => w.UserId == user!.Id)
                .OrderByDescending(w => w.AddedDate)
                .ToListAsync();

            return View(wishlistItems);
        }

        // POST: Wishlist/Toggle
        [HttpPost]
        public async Task<IActionResult> Toggle(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var existingItem = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == user.Id && w.GameAccountId == productId);

            if (existingItem != null)
            {
                // Remove from wishlist
                _context.Wishlists.Remove(existingItem);
                await _context.SaveChangesAsync();
                return Json(new { success = true, action = "removed" });
            }
            else
            {
                // Add to wishlist
                var wishlistItem = new Wishlist
                {
                    UserId = user.Id,
                    GameAccountId = productId
                };
                _context.Wishlists.Add(wishlistItem);
                await _context.SaveChangesAsync();
                return Json(new { success = true, action = "added" });
            }
        }

        // POST: Wishlist/Remove/5
        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var wishlistItem = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == user!.Id);

            if (wishlistItem != null)
            {
                _context.Wishlists.Remove(wishlistItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Wishlist/Count
        public async Task<IActionResult> Count()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { count = 0 });
            }

            var count = await _context.Wishlists
                .CountAsync(w => w.UserId == user.Id);

            return Json(new { count });
        }
    }
} 