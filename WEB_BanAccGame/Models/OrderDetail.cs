using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB_BanAccGame.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }
        
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; } = 1;
        
        [Display(Name = "Giá")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        
        // Foreign keys
        public int OrderId { get; set; }
        public int GameAccountId { get; set; }
        
        // Navigation properties
        [ForeignKey("OrderId")]
        public Order? Order { get; set; }
        
        [ForeignKey("GameAccountId")]
        public GameAccount? GameAccount { get; set; }
    }
} 