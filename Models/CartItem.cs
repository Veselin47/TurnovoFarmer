namespace TurnovoFarmer.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Quantity { get; set; } // Поддържа десетични кг (напр. 1.5 кг)
        public string UnitType { get; set; } = "kg";

        // Динамично изчисляване на общата сума за този продукт
        public decimal TotalPrice => Price * Quantity;
    }
}