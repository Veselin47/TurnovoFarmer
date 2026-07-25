namespace TurnovoFarmer.Models.ViewModels
{
    public class ManageScheduleViewModel
    {
        public int Id { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public string ZoneDeliveryDay { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; }
        public string DayOfWeekBg { get; set; } = string.Empty;
        public decimal CurrentTotalWeight { get; set; }
        public decimal MinWeightRequired { get; set; }
        public string Status { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public bool IsPast { get; set; }
        public bool CanDelete { get; set; }
        public bool CanClose { get; set; }
    }
}
