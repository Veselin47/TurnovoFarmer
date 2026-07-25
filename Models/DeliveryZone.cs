using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurnovoFarmer.Models
{
    public class DeliveryZone
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string ZoneName { get; set; } = string.Empty; 

        [Required]
        [StringLength(50)]
        public string DeliveryDay { get; set; } = string.Empty; 

        public string PickupLocationDescription { get; set; } = string.Empty; 

        [Column(TypeName = "decimal(10, 2)")]
        public decimal MinWeightRequired { get; set; } = 50.00m; 
    }
}