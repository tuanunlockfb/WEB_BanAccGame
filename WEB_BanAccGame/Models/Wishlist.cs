using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB_BanAccGame.Models
{
    public class Wishlist
    {
        public int Id { get; set; }

        [Display(Name = "Ngày thêm")]
        public DateTime AddedDate { get; set; } = DateTime.Now;

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