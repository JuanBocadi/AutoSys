using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSys.Models
{
    public class ServicioFijo
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public string? DescripcionPredeterminada { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PrecioSugerido { get; set; }

        // Navegación inversa
        public ICollection<Ingreso> Ingresos { get; set; } = new List<Ingreso>();
    }
}
