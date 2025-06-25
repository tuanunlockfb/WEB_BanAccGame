using System.ComponentModel.DataAnnotations;

namespace WEB_BanAccGame.Models
{
    // DTOs for API responses
    public class ProductDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Server { get; set; }
        public int? Level { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        
        // Database columns
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    public class CreateProductDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title must not exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Description must not exceed 2000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 999999999, ErrorMessage = "Price must be between 0.01 and 999,999,999")]
        public decimal Price { get; set; }

        [StringLength(100, ErrorMessage = "Server must not exceed 100 characters")]
        public string? Server { get; set; }

        public int? Level { get; set; }

        [StringLength(100000, ErrorMessage = "Image URL must not exceed 100000 characters")]
        public string? ImageUrl { get; set; }

        public bool IsAvailable { get; set; } = true;

        [Required(ErrorMessage = "Category ID is required")]
        public int CategoryId { get; set; }

        // Database columns
        [StringLength(100, ErrorMessage = "Username must not exceed 100 characters")]
        public string? Username { get; set; }

        [StringLength(100, ErrorMessage = "Password must not exceed 100 characters")]
        public string? Password { get; set; }
    }

    public class UpdateProductDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title must not exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Description must not exceed 2000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 999999999, ErrorMessage = "Price must be between 0.01 and 999,999,999")]
        public decimal Price { get; set; }

        [StringLength(100, ErrorMessage = "Server must not exceed 100 characters")]
        public string? Server { get; set; }

        public int? Level { get; set; }

        [StringLength(100000, ErrorMessage = "Image URL must not exceed 100000 characters")]
        public string? ImageUrl { get; set; }

        public bool IsAvailable { get; set; } = true;

        [Required(ErrorMessage = "Category ID is required")]
        public int CategoryId { get; set; }

        // Database columns
        [StringLength(100, ErrorMessage = "Username must not exceed 100 characters")]
        public string? Username { get; set; }

        [StringLength(100, ErrorMessage = "Password must not exceed 100 characters")]
        public string? Password { get; set; }
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public IEnumerable<string>? Errors { get; set; }
        public int? TotalCount { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public int? TotalPages { get; set; }
    }
} 