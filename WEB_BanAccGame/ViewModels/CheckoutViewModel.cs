using System.ComponentModel.DataAnnotations;
using WEB_BanAccGame.Models;

namespace WEB_BanAccGame.ViewModels
{
    public class CheckoutViewModel
    {
        public List<ShoppingCartItem> CartItems { get; set; } = new List<ShoppingCartItem>();
        public decimal TotalAmount { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }
    }
} 