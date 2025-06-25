using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB_BanAccGame.Models
{
    public class GameAccount
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Tên tài khoản là bắt buộc")]
        [Display(Name = "Tên tài khoản")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Display(Name = "Mô tả")]
        [StringLength(2000)]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Giá là bắt buộc")]
        [Display(Name = "Giá")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 999999999)]
        public decimal Price { get; set; }
        
        [Display(Name = "Server/Máy chủ")]
        [StringLength(100)]
        public string? Server { get; set; }
        
        [Display(Name = "Cấp độ")]
        public int? Level { get; set; }
        
        [Display(Name = "Hình ảnh")]
        [StringLength(100000)]
        public string? ImageUrl { get; set; }
        
        [Display(Name = "Trạng thái")]
        public bool IsAvailable { get; set; } = true;
        
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Database columns
        [StringLength(100)]
        [Display(Name = "Username")]
        public string? Username { get; set; }
        
        [StringLength(100)]
        [Display(Name = "Password")]
        public string? Password { get; set; }
        
        // Foreign key
        public int CategoryId { get; set; }
        
        // Navigation property
        [ForeignKey("CategoryId")]
        public GameCategory? Category { get; set; }
    }
} 