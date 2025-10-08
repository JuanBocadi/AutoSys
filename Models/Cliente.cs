using System.ComponentModel.DataAnnotations;

namespace AutoSys.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Nombre { get; set; } = string.Empty;
        [Required, StringLength(100)]
        public string Apellido { get; set; } = string.Empty;
        [StringLength(50)]
        public string Telefono { get; set; } = string.Empty;
        [StringLength(20)]
        public string? DNI { get; set; }
        [EmailAddress, StringLength(256)]
        public string? Email { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
    }
}
