using Microsoft.EntityFrameworkCore;
using WEB_BanAccGame.Data;
using WEB_BanAccGame.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace WEB_BanAccGame.Services
{
    public class ShoppingCart
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ShoppingCart>? _logger;
        public string ShoppingCartId { get; set; } = string.Empty;
        public List<ShoppingCartItem> ShoppingCartItems { get; set; } = new List<ShoppingCartItem>();

        private ShoppingCart(ApplicationDbContext context, ILogger<ShoppingCart>? logger = null)
        {
            _context = context;
            _logger = logger;
        }

        public static ShoppingCart GetCart(IServiceProvider services)
        {
            var httpContext = services.GetRequiredService<IHttpContextAccessor>()?.HttpContext;
            var session = httpContext?.Session;
            
            var context = services.GetService<ApplicationDbContext>() ?? throw new Exception("Error initializing");
            var logger = services.GetService<ILogger<ShoppingCart>>();
            
            // Try to get CartId from cookie first, then session
            string? cartId = httpContext?.Request.Cookies["CartId"];
            
            if (string.IsNullOrEmpty(cartId))
            {
                cartId = session?.GetString("CartId");
            }
            
            if (string.IsNullOrEmpty(cartId))
            {
                cartId = Guid.NewGuid().ToString();
                session?.SetString("CartId", cartId);
                
                // Also save to cookie
                httpContext?.Response.Cookies.Append("CartId", cartId, new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(7),
                    HttpOnly = true,
                    IsEssential = true
                });
            }
            else
            {
                // Ensure both session and cookie have the same CartId
                session?.SetString("CartId", cartId);
                if (httpContext?.Request.Cookies["CartId"] != cartId)
                {
                    httpContext?.Response.Cookies.Append("CartId", cartId, new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(7),
                        HttpOnly = true,
                        IsEssential = true
                    });
                }
            }
            
            logger?.LogInformation($"GetCart - CartId: {cartId}");
            
            return new ShoppingCart(context, logger) { ShoppingCartId = cartId };
        }

        public async Task AddToCartAsync(GameAccount gameAccount, int quantity = 1)
        {
            _logger?.LogInformation($"AddToCartAsync - CartId: {ShoppingCartId}, ProductId: {gameAccount.Id}");
            
            var shoppingCartItem = await _context.ShoppingCartItems
                .SingleOrDefaultAsync(s => s.GameAccount!.Id == gameAccount.Id && s.ShoppingCartId == ShoppingCartId);

            if (shoppingCartItem == null)
            {
                shoppingCartItem = new ShoppingCartItem
                {
                    ShoppingCartId = ShoppingCartId,
                    GameAccount = gameAccount,
                    GameAccountId = gameAccount.Id,
                    Quantity = quantity
                };

                _context.ShoppingCartItems.Add(shoppingCartItem);
                _logger?.LogInformation($"AddToCartAsync - Adding new item to cart");
            }
            else
            {
                shoppingCartItem.Quantity++;
                _logger?.LogInformation($"AddToCartAsync - Updating quantity to {shoppingCartItem.Quantity}");
            }
            
            await _context.SaveChangesAsync();
            _logger?.LogInformation($"AddToCartAsync - SaveChanges completed");
        }

        public async Task<int> RemoveFromCartAsync(GameAccount gameAccount)
        {
            var shoppingCartItem = await _context.ShoppingCartItems
                .SingleOrDefaultAsync(s => s.GameAccount!.Id == gameAccount.Id && s.ShoppingCartId == ShoppingCartId);

            var localAmount = 0;

            if (shoppingCartItem != null)
            {
                if (shoppingCartItem.Quantity > 1)
                {
                    shoppingCartItem.Quantity--;
                    localAmount = shoppingCartItem.Quantity;
                }
                else
                {
                    _context.ShoppingCartItems.Remove(shoppingCartItem);
                }
                
                await _context.SaveChangesAsync();
            }

            return localAmount;
        }

        public async Task<List<ShoppingCartItem>> GetShoppingCartItemsAsync()
        {
            var items = await _context.ShoppingCartItems
                .Where(c => c.ShoppingCartId == ShoppingCartId)
                .Include(s => s.GameAccount)
                .ThenInclude(p => p!.Category)
                .ToListAsync();
                
            // Debug logging
            _logger?.LogInformation($"GetShoppingCartItemsAsync - CartId: {ShoppingCartId}, Items count: {items.Count}");
            
            return ShoppingCartItems = items;
        }

        public async Task ClearCartAsync()
        {
            var cartItems = _context.ShoppingCartItems
                .Where(cart => cart.ShoppingCartId == ShoppingCartId);

            _context.ShoppingCartItems.RemoveRange(cartItems);
            
            await _context.SaveChangesAsync();
        }

        public async Task<decimal> GetShoppingCartTotalAsync()
        {
            var total = await _context.ShoppingCartItems
                .Where(c => c.ShoppingCartId == ShoppingCartId)
                .Select(c => c.GameAccount!.Price * c.Quantity)
                .SumAsync();
                
            return total;
        }

        public async Task<int> GetCartItemCountAsync()
        {
            return await _context.ShoppingCartItems
                .Where(c => c.ShoppingCartId == ShoppingCartId)
                .SumAsync(c => c.Quantity);
        }
    }
} 