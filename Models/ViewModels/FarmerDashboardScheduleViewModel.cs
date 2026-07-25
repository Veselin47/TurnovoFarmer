namespace TurnovoFarmer.Models.ViewModels
{
    public class FarmerDashboardScheduleViewModel
    {
        public int Id { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public string DeliveryDay { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal CurrentTotalWeight { get; set; }
        public decimal MinWeightRequired { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
