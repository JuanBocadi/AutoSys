using System.ComponentModel.DataAnnotations;

namespace AutoSys.Models
{
    public class Vehiculo
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "La patente es obligatoria.")]
        [StringLength(20, ErrorMessage = "La patente no puede exceder los 20 caracteres.")]
        public string Patente { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "La marca es obligatoria.")]
        [StringLength(100, ErrorMessage = "La marca no puede exceder los 100 caracteres.")]
        public string Marca { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El modelo es obligatorio.")]
        [StringLength(100, ErrorMessage = "El modelo no puede exceder los 100 caracteres.")]
        public string Modelo { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Debe seleccionar un cliente.")]
        public int ClienteId { get; set; }
        
        public Cliente? Cliente { get; set; }
        
        public ICollection<Ingreso> Ingresos { get; set; } = new List<Ingreso>();
        public ICollection<HistorialPropietario> HistorialPropietarios { get; set; } = new List<HistorialPropietario>();
    }
}
