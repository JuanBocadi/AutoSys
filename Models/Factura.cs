using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSys.Models
{
    public class Factura
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El número de factura es requerido")]
        [StringLength(50)]
        [Display(Name = "Número de Factura")]
        public string NumeroFactura { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Fecha de Emisión")]
        public DateTime FechaEmision { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Ingreso")]
        public int IngresoId { get; set; }

        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }

        [StringLength(20)]
        [Display(Name = "Método de Pago")]
        public string? MetodoPago { get; set; } // Efectivo, Tarjeta, Transferencia

        [StringLength(20)]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Pagada, Anulada

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Subtotal")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "IVA")]
        public decimal IVA { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total")]
        public decimal Total { get; set; }

        [StringLength(500)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        // Navegación
        public Ingreso? Ingreso { get; set; }
        public Cliente? Cliente { get; set; }
        public ICollection<DetalleFactura> Detalles { get; set; } = new List<DetalleFactura>();
    }
}
