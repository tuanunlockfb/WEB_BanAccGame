using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB_BanAccGame.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nội dung đánh giá là bắt buộc")]
        [Display(Name = "Nội dung")]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Đánh giá sao là bắt buộc")]
        [Display(Name = "Đánh giá")]
        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao")]
        public int Rating { get; set; }

        [Display(Name = "Ngày đánh giá")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Đã xác minh mua hàng")]
        public bool IsVerifiedPurchase { get; set; }

        // Foreign keys
        public string UserId { get; set; } = string.Empty;
        public int GameAccountId { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [ForeignKey("GameAccountId")]
        public GameAccount? GameAccount { get; set; }
    }
} 