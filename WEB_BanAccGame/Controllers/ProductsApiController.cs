using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_BanAccGame.Data;
using WEB_BanAccGame.Models;
using Microsoft.AspNetCore.Authorization;

namespace WEB_BanAccGame.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductsApiController> _logger;

        public ProductsApiController(ApplicationDbContext context, ILogger<ProductsApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/ProductsApi
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] int? categoryId = null,
            [FromQuery] bool? isAvailable = null,
            [FromQuery] string? sortBy = "CreatedDate",
            [FromQuery] string? sortOrder = "desc")
        {
            try
            {
                var query = _context.GameAccounts.Include(g => g.Category).AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(g => g.Title.Contains(search) || 
                                           g.Description!.Contains(search));
                }

                if (categoryId.HasValue)
                {
                    query = query.Where(g => g.CategoryId == categoryId.Value);
                }

                if (isAvailable.HasValue)
                {
                    query = query.Where(g => g.IsAvailable == isAvailable.Value);
                }

                // Apply sorting
                query = sortBy?.ToLower() switch
                {
                    "title" => sortOrder?.ToLower() == "desc" 
                        ? query.OrderByDescending(g => g.Title)
                        : query.OrderBy(g => g.Title),
                    "price" => sortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(g => g.Price)
                        : query.OrderBy(g => g.Price),
                    "createddate" => sortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(g => g.CreatedDate)
                        : query.OrderBy(g => g.CreatedDate),
                    _ => query.OrderByDescending(g => g.CreatedDate)
                };

                // Get total count for pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                var products = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(g => new ProductDto
                    {
                        Id = g.Id,
                        Title = g.Title,
                        Description = g.Description,
                        Price = g.Price,
                        Server = g.Server,
                        Level = g.Level,
                        ImageUrl = g.ImageUrl,
                        IsAvailable = g.IsAvailable,
                        CreatedDate = g.CreatedDate,
                        CategoryId = g.CategoryId,
                        CategoryName = g.Category!.Name,
                        Username = g.Username,
                        Password = g.Password
                    })
                    .ToListAsync();

                var response = new ApiResponse<IEnumerable<ProductDto>>
                {
                    Success = true,
                    Data = products,
                    Message = "Products retrieved successfully",
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return StatusCode(500, new ApiResponse<IEnumerable<ProductDto>>
                {
                    Success = false,
                    Message = "Internal server error occurred while retrieving products"
                });
            }
        }

        // GET: api/ProductsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> GetProduct(int id)
        {
            try
            {
                var product = await _context.GameAccounts
                    .Include(g => g.Category)
                    .Where(g => g.Id == id)
                    .Select(g => new ProductDto
                    {
                        Id = g.Id,
                        Title = g.Title,
                        Description = g.Description,
                        Price = g.Price,
                        Server = g.Server,
                        Level = g.Level,
                        ImageUrl = g.ImageUrl,
                        IsAvailable = g.IsAvailable,
                        CreatedDate = g.CreatedDate,
                        CategoryId = g.CategoryId,
                        CategoryName = g.Category!.Name,
                        Username = g.Username,
                        Password = g.Password
                    })
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    return NotFound(new ApiResponse<ProductDto>
                    {
                        Success = false,
                        Message = $"Product with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<ProductDto>
                {
                    Success = true,
                    Data = product,
                    Message = "Product retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product with ID {ProductId}", id);
                return StatusCode(500, new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "Internal server error occurred while retrieving the product"
                });
            }
        }

        // POST: api/ProductsApi
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<ProductDto>
                    {
                        Success = false,
                        Message = "Invalid product data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                // Check if category exists
                var categoryExists = await _context.GameCategories.AnyAsync(c => c.Id == createProductDto.CategoryId);
                if (!categoryExists)
                {
                    return BadRequest(new ApiResponse<ProductDto>
                    {
                        Success = false,
                        Message = "Invalid category ID"
                    });
                }

                var gameAccount = new GameAccount
                {
                    Title = createProductDto.Title,
                    Description = createProductDto.Description,
                    Price = createProductDto.Price,
                    Server = createProductDto.Server,
                    Level = createProductDto.Level,
                    ImageUrl = createProductDto.ImageUrl,
                    IsAvailable = createProductDto.IsAvailable,
                    CategoryId = createProductDto.CategoryId,
                    Username = createProductDto.Username,
                    Password = createProductDto.Password,
                    CreatedDate = DateTime.Now
                };

                _context.GameAccounts.Add(gameAccount);
                await _context.SaveChangesAsync();

                // Reload with category information
                await _context.Entry(gameAccount)
                    .Reference(g => g.Category)
                    .LoadAsync();

                var productDto = new ProductDto
                {
                    Id = gameAccount.Id,
                    Title = gameAccount.Title,
                    Description = gameAccount.Description,
                    Price = gameAccount.Price,
                    Server = gameAccount.Server,
                    Level = gameAccount.Level,
                    ImageUrl = gameAccount.ImageUrl,
                    IsAvailable = gameAccount.IsAvailable,
                    CreatedDate = gameAccount.CreatedDate,
                    CategoryId = gameAccount.CategoryId,
                    CategoryName = gameAccount.Category?.Name,
                    Username = gameAccount.Username,
                    Password = gameAccount.Password
                };

                return CreatedAtAction(nameof(GetProduct), new { id = gameAccount.Id }, 
                    new ApiResponse<ProductDto>
                    {
                        Success = true,
                        Data = productDto,
                        Message = "Product created successfully"
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "Internal server error occurred while creating the product"
                });
            }
        }

        // PUT: api/ProductsApi/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<ProductDto>
                    {
                        Success = false,
                        Message = "Invalid product data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var gameAccount = await _context.GameAccounts.FindAsync(id);
                if (gameAccount == null)
                {
                    return NotFound(new ApiResponse<ProductDto>
                    {
                        Success = false,
                        Message = $"Product with ID {id} not found"
                    });
                }

                // Check if category exists
                var categoryExists = await _context.GameCategories.AnyAsync(c => c.Id == updateProductDto.CategoryId);
                if (!categoryExists)
                {
                    return BadRequest(new ApiResponse<ProductDto>
                    {
                        Success = false,
                        Message = "Invalid category ID"
                    });
                }

                // Update properties
                gameAccount.Title = updateProductDto.Title;
                gameAccount.Description = updateProductDto.Description;
                gameAccount.Price = updateProductDto.Price;
                gameAccount.Server = updateProductDto.Server;
                gameAccount.Level = updateProductDto.Level;
                gameAccount.ImageUrl = updateProductDto.ImageUrl;
                gameAccount.IsAvailable = updateProductDto.IsAvailable;
                gameAccount.CategoryId = updateProductDto.CategoryId;
                gameAccount.Username = updateProductDto.Username;
                gameAccount.Password = updateProductDto.Password;

                await _context.SaveChangesAsync();

                // Reload with category information
                await _context.Entry(gameAccount)
                    .Reference(g => g.Category)
                    .LoadAsync();

                var productDto = new ProductDto
                {
                    Id = gameAccount.Id,
                    Title = gameAccount.Title,
                    Description = gameAccount.Description,
                    Price = gameAccount.Price,
                    Server = gameAccount.Server,
                    Level = gameAccount.Level,
                    ImageUrl = gameAccount.ImageUrl,
                    IsAvailable = gameAccount.IsAvailable,
                    CreatedDate = gameAccount.CreatedDate,
                    CategoryId = gameAccount.CategoryId,
                    CategoryName = gameAccount.Category?.Name,
                    Username = gameAccount.Username,
                    Password = gameAccount.Password
                };

                return Ok(new ApiResponse<ProductDto>
                {
                    Success = true,
                    Data = productDto,
                    Message = "Product updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID {ProductId}", id);
                return StatusCode(500, new ApiResponse<ProductDto>
                {
                    Success = false,
                    Message = "Internal server error occurred while updating the product"
                });
            }
        }

        // DELETE: api/ProductsApi/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteProduct(int id)
        {
            try
            {
                var gameAccount = await _context.GameAccounts.FindAsync(id);
                if (gameAccount == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Product with ID {id} not found"
                    });
                }

                // Check if product is referenced in any orders
                var hasOrders = await _context.OrderDetails.AnyAsync(od => od.GameAccountId == id);
                if (hasOrders)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Cannot delete product as it is referenced in existing orders"
                    });
                }

                _context.GameAccounts.Remove(gameAccount);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Product deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID {ProductId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error occurred while deleting the product"
                });
            }
        }

        // GET: api/ProductsApi/categories
        [HttpGet("categories")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetCategories()
        {
            try
            {
                var categories = await _context.GameCategories
                    .Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        IconUrl = c.IconUrl
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<IEnumerable<CategoryDto>>
                {
                    Success = true,
                    Data = categories,
                    Message = "Categories retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return StatusCode(500, new ApiResponse<IEnumerable<CategoryDto>>
                {
                    Success = false,
                    Message = "Internal server error occurred while retrieving categories"
                });
            }
        }
    }
}
