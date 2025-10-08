using System.ComponentModel.DataAnnotations;

namespace AutoSys.Models
{
    public class Vehiculo
    {
        public int Id { get; set; }
        [Required, StringLength(20)]
        public string Patente { get; set; } = string.Empty;
        [Required, StringLength(100)]
        public string Marca { get; set; } = string.Empty;
        [Required, StringLength(100)]
        public string Modelo { get; set; } = string.Empty;
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;
        public ICollection<Ingreso> Ingresos { get; set; } = new List<Ingreso>();
    }
}
