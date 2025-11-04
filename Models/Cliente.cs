using System.ComponentModel.DataAnnotations;

namespace AutoSys.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(100, ErrorMessage = "El apellido no puede exceder los 100 caracteres.")]
        public string Apellido { get; set; } = string.Empty;
        
        [StringLength(50, ErrorMessage = "El teléfono no puede exceder los 50 caracteres.")]
        public string Telefono { get; set; } = string.Empty;
        
        [StringLength(20, ErrorMessage = "El DNI no puede exceder los 20 caracteres.")]
        public string? DNI { get; set; }
        
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        [StringLength(256, ErrorMessage = "El correo electrónico no puede exceder los 256 caracteres.")]
        public string? Email { get; set; }
        
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        
        public ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
    }
}
