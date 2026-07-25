using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurnovoFarmer.Models
{
    public class DeliverySchedule
    {
        public int Id { get; set; }

        [Required]
        public int ZoneId { get; set; }

        [ForeignKey("ZoneId")]
        public DeliveryZone? Zone { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DeliveryDate { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal CurrentTotalWeight { get; set; } = 0.00m;

        [Required]
        public string Status { get; set; } = "collecting";

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}