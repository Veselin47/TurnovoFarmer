namespace TurnovoFarmer.Models.ViewModels
{
    public class HarvestAggregateViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public string UnitType { get; set; } = string.Empty;
        public decimal TotalQuantity { get; set; }
    }
}
