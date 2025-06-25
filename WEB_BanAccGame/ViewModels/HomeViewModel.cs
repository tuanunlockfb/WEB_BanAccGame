using WEB_BanAccGame.Models;

namespace WEB_BanAccGame.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<GameCategory> Categories { get; set; } = new List<GameCategory>();
        public IEnumerable<GameAccount> FeaturedAccounts { get; set; } = new List<GameAccount>();
        public IEnumerable<GameAccount> NewAccounts { get; set; } = new List<GameAccount>();
    }
} 