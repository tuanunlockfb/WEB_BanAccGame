using System.ComponentModel.DataAnnotations;

namespace WEB_BanAccGame.Models
{
    public class ShoppingCartItem
    {
        public int Id { get; set; }
        
        public int GameAccountId { get; set; }
        public GameAccount? GameAccount { get; set; }
        
        public int Quantity { get; set; } = 1;
        
        [StringLength(450)]
        public string ShoppingCartId { get; set; } = string.Empty;
    }
} 