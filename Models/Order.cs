using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurnovoFarmer.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        [Required]
        [StringLength(150)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required]
        public int ScheduleId { get; set; }

        [ForeignKey("ScheduleId")]
        public DeliverySchedule? Schedule { get; set; }

        [Required]
        public string DeliveryType { get; set; } = "address"; 

        public string? DeliveryAddress { get; set; } 

        
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = "cash"; // cash, card

        [Required]
        public string PaymentStatus { get; set; } = "pending"; // pending, paid, failed, refunded

        [Required]
        public string OrderStatus { get; set; } = "pending_course"; // pending_course, processing, in_transit, delivered, cancelled

        public string? StripePaymentIntentId { get; set; } // Идентификатор за сигурност от Stripe

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}