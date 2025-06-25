using WEB_BanAccGame.Services;

namespace WEB_BanAccGame.ViewModels
{
    public class ShoppingCartViewModel
    {
        public ShoppingCart ShoppingCart { get; set; } = null!;
        public decimal ShoppingCartTotal { get; set; }
        public int ItemCount { get; set; }
    }
} 