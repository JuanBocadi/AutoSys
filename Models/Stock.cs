using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSys.Models
{
    public class Stock
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder los 200 caracteres.")]
        public string Nombre { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "La descripción no puede exceder los 1000 caracteres.")]
        public string? Descripcion { get; set; }
        
        [Required(ErrorMessage = "La cantidad es obligatoria.")]
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser un valor positivo.")]
        public int Cantidad { get; set; }
        
        [Required(ErrorMessage = "El stock mínimo es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo debe ser un valor positivo.")]
        public int StockMinimo { get; set; }
        
        [StringLength(50, ErrorMessage = "La unidad no puede exceder los 50 caracteres.")]
        public string? Unidad { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "El precio unitario debe ser un valor positivo.")]
        public decimal PrecioUnitario { get; set; }
        
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;

        [NotMapped]
        public string ColorSemaforo
        {
            get
            {
                if (Cantidad > StockMinimo * 2)
                    return "success";
                
                if (Cantidad >= StockMinimo && Cantidad <= StockMinimo * 2)
                    return "warning";
                
                return "danger";
            }
        }

        [NotMapped]
        public string NivelStock
        {
            get
            {
                if (Cantidad > StockMinimo * 2)
                    return "Suficiente";
                
                if (Cantidad >= StockMinimo && Cantidad <= StockMinimo * 2)
                    return "Bajo";
                
                return "Crítico";
            }
        }
    }
}
