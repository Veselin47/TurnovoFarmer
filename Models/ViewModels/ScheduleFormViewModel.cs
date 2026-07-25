using System.ComponentModel.DataAnnotations;

namespace TurnovoFarmer.Models.ViewModels
{
    public class ScheduleFormViewModel
    {
        [Required(ErrorMessage = "Моля, изберете зона за доставка.")]
        [Display(Name = "Зона за доставка")]
        public int ZoneId { get; set; }

        [Required(ErrorMessage = "Моля, изберете дата за доставка.")]
        [DataType(DataType.Date)]
        [Display(Name = "Дата на доставка")]
        public DateTime DeliveryDate { get; set; } = DateTime.Today.AddDays(1);
    }
}
