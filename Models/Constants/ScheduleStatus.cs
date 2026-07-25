namespace TurnovoFarmer.Models.Constants
{
    public static class ScheduleStatus
    {
        public const string Collecting = "collecting";
        public const string Confirmed = "confirmed";
        public const string Closed = "closed";
        public const string Completed = "completed";
        public const string Cancelled = "cancelled";

        /// <summary>Statuses that still accept new customer orders.</summary>
        public static readonly string[] OpenForOrders = [Collecting];

        /// <summary>Statuses hidden from the client checkout dropdown.</summary>
        public static readonly string[] ClosedToClients =
            [Confirmed, Closed, Completed, Cancelled];
    }
}
