using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSys.Models
{
    public class Ingreso
    {
        public int Id { get; set; }
        public DateTime FechaIngreso { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Debe ingresar un diagnóstico.")]
        [Column("DiagnosticoInicial")]
        [StringLength(2000)]
        public string Diagnostico { get; set; } = string.Empty;

        [StringLength(512)]
        public string? FotoPath { get; set; }
        public DateTime? FechaEgreso { get; set; }
        [Required, StringLength(50)]
        public string Estado { get; set; } = "En revisión";

        [Required(ErrorMessage = "Debe seleccionar un vehículo.")]
        public int? VehiculoId { get; set; }
        public Vehiculo? Vehiculo { get; set; }
        public ICollection<FotoVehiculo>? Fotos { get; set; }
    }
}