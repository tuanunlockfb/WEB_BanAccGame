using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WEB_BanAccGame.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "Họ và tên")]
        [StringLength(100)]
        public string? FullName { get; set; }
        
        [Display(Name = "Địa chỉ")]
        [StringLength(200)]
        public string? Address { get; set; }
        
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Navigation property
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
} 