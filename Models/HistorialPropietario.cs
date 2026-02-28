using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSys.Models
{
    public class HistorialPropietario
    {
        public int Id { get; set; }

        [Required]
        public int VehiculoId { get; set; }
        public Vehiculo? Vehiculo { get; set; }

        [Required]
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        [Required]
        [Display(Name = "Fecha Desde")]
        public DateTime FechaDesde { get; set; } = DateTime.Now;

        [Display(Name = "Fecha Hasta")]
        public DateTime? FechaHasta { get; set; }

        public bool EsPropietarioActual { get; set; } = true;
    }
}
