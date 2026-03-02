using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSys.Models
{
    public class DetalleFactura
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una factura.")]
        [Display(Name = "Factura")]
        public int FacturaId { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(200, ErrorMessage = "La descripción no puede exceder los 200 caracteres.")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "El tipo no puede exceder los 20 caracteres.")]
        [Display(Name = "Tipo")]
        public string Tipo { get; set; } = "Servicio"; // Servicio, Repuesto

        [Required(ErrorMessage = "La cantidad es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; } = 1;

        [Required(ErrorMessage = "El precio unitario es obligatorio.")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "El precio unitario debe ser un valor positivo.")]
        [Display(Name = "Precio Unitario")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "El subtotal debe ser un valor positivo.")]
        [Display(Name = "Subtotal")]
        public decimal Subtotal { get; set; }

        // FK opcional hacia Stock (solo si la línea es un repuesto del inventario)
        [Display(Name = "Repuesto")]
        public int? StockId { get; set; }
        public Stock? Stock { get; set; }

        // Navegación
        public Factura? Factura { get; set; }
    }
}
