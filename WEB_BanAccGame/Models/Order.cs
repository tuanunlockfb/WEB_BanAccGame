using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEB_BanAccGame.Models
{
    public class Order
    {
        public int Id { get; set; }
        
        [Display(Name = "Mã đơn hàng")]
        [StringLength(50)]
        public string OrderCode { get; set; } = string.Empty;
        
        [Display(Name = "Ngày đặt")]
        public DateTime OrderDate { get; set; } = DateTime.Now;
        
        [Display(Name = "Tổng tiền")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        
        [Display(Name = "Trạng thái")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        
        [Display(Name = "Ghi chú")]
        [StringLength(500)]
        public string? Notes { get; set; }
        
        // Foreign key
        public string UserId { get; set; } = string.Empty;
        
        // Navigation properties
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
        
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
    
    public enum OrderStatus
    {
        [Display(Name = "Đang chờ")]
        Pending,
        [Display(Name = "Đã thanh toán")]
        Paid,
        [Display(Name = "Đã hoàn thành")]
        Completed,
        [Display(Name = "Đã hủy")]
        Cancelled
    }
} 