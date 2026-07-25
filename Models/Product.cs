using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurnovoFarmer.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        [Required(ErrorMessage = "Name of product is required.")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal PricePerUnit { get; set; }

        [Required]
        public string UnitType { get; set; } = "kg"; // kg, piece, jar

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal StockQuantity { get; set; }

        public string ImageUrl { get; set; } = "/images/products/default.jpg";

        public bool IsActive { get; set; } = true;

       
        public DateTime? DeletedAt { get; set; }
    }
}