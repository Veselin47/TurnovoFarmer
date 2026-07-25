namespace TurnovoFarmer.Models.ViewModels
{
    public class DriverDeliverySheetViewModel
    {
        public int ScheduleId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public string DeliveryDay { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal CurrentTotalWeight { get; set; }
        public decimal MinWeightRequired { get; set; }

        public List<HarvestAggregateViewModel> HarvestTotals { get; set; } = [];
        public List<Models.Order> Orders { get; set; } = [];
    }
}
