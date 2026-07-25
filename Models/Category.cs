using Microsoft.CodeAnalysis;
using System.ComponentModel.DataAnnotations;

namespace TurnovoFarmer.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name of category is required.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Slug { get; set; } = string.Empty; // URL адреси (напр. /catalog/zelenchutsi)

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}