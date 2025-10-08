using System.ComponentModel.DataAnnotations;

namespace AutoSys.Models
{
    public class FotoVehiculo
    {
        public int Id { get; set; }
        public int IngresoId { get; set; }
        [Required, StringLength(20)]
        public string Tipo { get; set; } = "Ingreso"; // Ingreso y Egreso
        [Required, StringLength(512)]
        public string RutaArchivo { get; set; } = string.Empty;
        public DateTime FechaCarga { get; set; } = DateTime.Now;
        public Ingreso Ingreso { get; set; } = null!;
    }
}


