using System.ComponentModel.DataAnnotations;

namespace WEB_BanAccGame.Models
{
    public class GameCategory
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Tên thể loại là bắt buộc")]
        [Display(Name = "Tên thể loại")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Display(Name = "Mô tả")]
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Display(Name = "Icon URL")]
        public string? IconUrl { get; set; }
        
        // Navigation property
        public ICollection<GameAccount> GameAccounts { get; set; } = new List<GameAccount>();
    }
} 