using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSys.Models
{
    public class Stock
    {
        public int Id { get; set; }
        
        [Required, StringLength(200)]
        public string Nombre { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Descripcion { get; set; }
        
        [Required]
        public int Cantidad { get; set; }
        
        [Required]
        public int StockMinimo { get; set; }
        
        [StringLength(50)]
        public string? Unidad { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }
        
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;

        // Propiedades calculadas para semaforización
        [NotMapped]
        public string ColorSemaforo
        {
            get
            {
                // Semaforización por nivel de stock
                // Verde: Suficiente (Cantidad > StockMinimo * 2)
                // Amarillo: Bajo (Cantidad entre StockMinimo y StockMinimo * 2)
                // Rojo: Crítico (Cantidad < StockMinimo)
                
                if (Cantidad > StockMinimo * 2)
                    return "success"; // Verde - Suficiente
                
                if (Cantidad >= StockMinimo && Cantidad <= StockMinimo * 2)
                    return "warning"; // Amarillo - Bajo
                
                return "danger"; // Rojo - Crítico
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
