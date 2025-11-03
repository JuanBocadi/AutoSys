using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSys.Models
{
    public class DetalleFactura
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Factura")]
        public int FacturaId { get; set; }

        [Required(ErrorMessage = "La descripción es requerida")]
        [StringLength(200)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Tipo")]
        public string Tipo { get; set; } = "Servicio"; // Servicio, Repuesto

        [Required]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; } = 1;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Precio Unitario")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Subtotal")]
        public decimal Subtotal { get; set; }

        // Navegación
        public Factura? Factura { get; set; }
    }
}
