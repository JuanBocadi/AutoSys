using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSys.Models
{
    public class Factura
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El número de factura es obligatorio.")]
        [StringLength(50, ErrorMessage = "El número de factura no puede exceder los 50 caracteres.")]
        [Display(Name = "Número de Factura")]
        public string NumeroFactura { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de emisión es obligatoria.")]
        [Display(Name = "Fecha de Emisión")]
        public DateTime FechaEmision { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Debe seleccionar un ingreso.")]
        [Display(Name = "Ingreso")]
        public int IngresoId { get; set; }

        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }

        [StringLength(20, ErrorMessage = "El método de pago no puede exceder los 20 caracteres.")]
        [Display(Name = "Método de Pago")]
        public string? MetodoPago { get; set; } // Efectivo, Tarjeta, Transferencia

        [StringLength(20, ErrorMessage = "El estado no puede exceder los 20 caracteres.")]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Pagada, Anulada

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "El subtotal debe ser un valor positivo.")]
        [Display(Name = "Subtotal")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "El IVA debe ser un valor positivo.")]
        [Display(Name = "IVA")]
        public decimal IVA { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "El total debe ser un valor positivo.")]
        [Display(Name = "Total")]
        public decimal Total { get; set; }

        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder los 500 caracteres.")]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        // Navegación
        public Ingreso? Ingreso { get; set; }
        public Cliente? Cliente { get; set; }
        public ICollection<DetalleFactura> Detalles { get; set; } = new List<DetalleFactura>();
    }
}
