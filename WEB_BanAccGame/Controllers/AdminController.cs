using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WEB_BanAccGame.Data;
using WEB_BanAccGame.Models;

namespace WEB_BanAccGame.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Admin
        public async Task<IActionResult> Index()
        {
            var viewModel = new
            {
                TotalProducts = await _context.GameAccounts.CountAsync(),
                TotalOrders = await _context.Orders.CountAsync(),
                TotalUsers = await _context.Users.CountAsync(),
                Revenue = await _context.Orders.Where(o => o.Status == OrderStatus.Completed).SumAsync(o => o.TotalAmount),
                RecentOrders = await _context.Orders
                    .Include(o => o.User)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToListAsync(),
                LowStockProducts = await _context.GameAccounts
                    .Where(g => g.IsAvailable)
                    .OrderByDescending(g => g.Price)
                    .Take(5)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // GET: Admin/Products
        public async Task<IActionResult> Products(string searchString, int? categoryId, int page = 1)
        {
            var query = _context.GameAccounts.Include(g => g.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(g => g.Title.Contains(searchString));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(g => g.CategoryId == categoryId);
            }

            const int pageSize = 10;
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var products = await query
                .OrderByDescending(g => g.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Categories = new SelectList(await _context.GameCategories.ToListAsync(), "Id", "Name", categoryId);
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString;
            ViewBag.CategoryId = categoryId;

            return View(products);
        }

        // GET: Admin/CreateProduct
        public async Task<IActionResult> CreateProduct()
        {
            ViewBag.Categories = new SelectList(await _context.GameCategories.ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: Admin/CreateProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(GameAccount gameAccount, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null)
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images/products");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    gameAccount.ImageUrl = "/images/products/" + uniqueFileName;
                }

                gameAccount.CreatedDate = DateTime.Now;
                _context.Add(gameAccount);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction(nameof(Products));
            }

            ViewBag.Categories = new SelectList(await _context.GameCategories.ToListAsync(), "Id", "Name", gameAccount.CategoryId);
            return View(gameAccount);
        }

        // GET: Admin/EditProduct/5
        public async Task<IActionResult> EditProduct(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gameAccount = await _context.GameAccounts.FindAsync(id);
            if (gameAccount == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(await _context.GameCategories.ToListAsync(), "Id", "Name", gameAccount.CategoryId);
            return View(gameAccount);
        }

        // POST: Admin/EditProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, GameAccount gameAccount, IFormFile? imageFile)
        {
            if (id != gameAccount.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.GameAccounts.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
                    
                    if (imageFile != null)
                    {
                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images/products");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        gameAccount.ImageUrl = "/images/products/" + uniqueFileName;

                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(existingProduct?.ImageUrl) && existingProduct.ImageUrl.StartsWith("/images/"))
                        {
                            string oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, existingProduct.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }
                    }
                    else
                    {
                        gameAccount.ImageUrl = existingProduct?.ImageUrl;
                    }

                    gameAccount.CreatedDate = existingProduct?.CreatedDate ?? DateTime.Now;
                    _context.Update(gameAccount);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật sản phẩm thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GameAccountExists(gameAccount.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Products));
            }

            ViewBag.Categories = new SelectList(await _context.GameCategories.ToListAsync(), "Id", "Name", gameAccount.CategoryId);
            return View(gameAccount);
        }

        // POST: Admin/DeleteProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var gameAccount = await _context.GameAccounts.FindAsync(id);
            if (gameAccount != null)
            {
                // Delete image if exists
                if (!string.IsNullOrEmpty(gameAccount.ImageUrl) && gameAccount.ImageUrl.StartsWith("/images/"))
                {
                    string imagePath = Path.Combine(_hostEnvironment.WebRootPath, gameAccount.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.GameAccounts.Remove(gameAccount);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa sản phẩm thành công!";
            }

            return RedirectToAction(nameof(Products));
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Orders(string status, string searchString, int page = 1)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<OrderStatus>(status, out var orderStatus))
                {
                    query = query.Where(o => o.Status == orderStatus);
                }
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(o => o.OrderCode.Contains(searchString) || 
                                       o.User!.Email!.Contains(searchString));
            }

            const int pageSize = 20;
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentStatus = status;
            ViewBag.SearchString = searchString;

            return View(orders);
        }

        // GET: Admin/OrderDetails/5
        public async Task<IActionResult> OrderDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.GameAccount)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Admin/UpdateOrderStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật trạng thái đơn hàng thành công!";
            }

            return RedirectToAction(nameof(OrderDetails), new { id = orderId });
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.GameCategories
                .Include(c => c.GameAccounts)
                .ToListAsync();
            return View(categories);
        }

        // POST: Admin/CreateCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(GameCategory category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm danh mục thành công!";
            }

            return RedirectToAction(nameof(Categories));
        }

        // POST: Admin/EditCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, GameCategory category)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật danh mục thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return RedirectToAction(nameof(Categories));
        }

        // POST: Admin/DeleteCategory/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.GameCategories
                .Include(c => c.GameAccounts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category != null)
            {
                if (category.GameAccounts.Any())
                {
                    TempData["Error"] = "Không thể xóa danh mục có sản phẩm!";
                }
                else
                {
                    _context.GameCategories.Remove(category);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Xóa danh mục thành công!";
                }
            }

            return RedirectToAction(nameof(Categories));
        }

        private bool GameAccountExists(int id)
        {
            return _context.GameAccounts.Any(e => e.Id == id);
        }

        private bool CategoryExists(int id)
        {
            return _context.GameCategories.Any(e => e.Id == id);
        }
    }
} 