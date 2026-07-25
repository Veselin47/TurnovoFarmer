using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurnovoFarmer.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Quantity { get; set; } 

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal PriceAtPurchase { get; set; } // Пази историческата цена в момента на покупката
    }
}