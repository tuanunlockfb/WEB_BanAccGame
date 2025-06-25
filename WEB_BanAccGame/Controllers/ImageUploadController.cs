using Microsoft.AspNetCore.Mvc;

namespace WEB_BanAccGame.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageUploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ImageUploadController> _logger;

        public ImageUploadController(IWebHostEnvironment environment, ILogger<ImageUploadController> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "Không có file được chọn" });
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return BadRequest(new { success = false, message = "Chỉ chấp nhận file hình ảnh (JPG, PNG, GIF, WebP)" });
                }

                // Validate file size (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { success = false, message = "File quá lớn. Tối đa 5MB" });
                }

                // Create products directory if it doesn't exist
                var uploadsDir = Path.Combine(_environment.WebRootPath, "images", "products");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                // Generate unique filename
                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsDir, uniqueFileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return relative URL
                var imageUrl = $"/images/products/{uniqueFileName}";

                return Ok(new
                {
                    success = true,
                    message = "Upload thành công",
                    imageUrl = imageUrl,
                    fileName = uniqueFileName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                return StatusCode(500, new { success = false, message = "Lỗi server khi upload file" });
            }
        }

        [HttpDelete("delete/{fileName}")]
        public IActionResult DeleteImage(string fileName)
        {
            try
            {
                // Validate filename to prevent directory traversal
                if (string.IsNullOrEmpty(fileName) || fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
                {
                    return BadRequest(new { success = false, message = "Tên file không hợp lệ" });
                }

                var filePath = Path.Combine(_environment.WebRootPath, "images", "products", fileName);
                
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    return Ok(new { success = true, message = "Xóa file thành công" });
                }
                else
                {
                    return NotFound(new { success = false, message = "File không tồn tại" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image: {FileName}", fileName);
                return StatusCode(500, new { success = false, message = "Lỗi server khi xóa file" });
            }
        }
    }
} 