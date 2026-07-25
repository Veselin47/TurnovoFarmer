using System.ComponentModel.DataAnnotations;

namespace TurnovoFarmer.Models.ViewModels
{
    public class ProductFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Моля, въведете име на продукта.")]
        [StringLength(150, ErrorMessage = "Името не може да надвишава 150 символа.")]
        [Display(Name = "Име на продукта")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Описание")]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Моля, въведете цена.")]
        [Range(0.01, 99999, ErrorMessage = "Цената трябва да е между 0.01 и 99999.")]
        [Display(Name = "Цена за единица (лв.)")]
        public decimal PricePerUnit { get; set; }

        [Required(ErrorMessage = "Моля, изберете единица.")]
        [Display(Name = "Единица")]
        public string UnitType { get; set; } = "kg";

        [Required(ErrorMessage = "Моля, въведете наличност.")]
        [Range(0, 99999, ErrorMessage = "Наличността трябва да е между 0 и 99999.")]
        [Display(Name = "Наличност")]
        public decimal StockQuantity { get; set; }

        [Display(Name = "URL на снимка")]
        public string ImageUrl { get; set; } = "/images/products/default.jpg";

        [Required(ErrorMessage = "Моля, изберете категория.")]
        [Display(Name = "Категория")]
        public int CategoryId { get; set; }

        [Display(Name = "Активен продукт")]
        public bool IsActive { get; set; } = true;
    }
}
