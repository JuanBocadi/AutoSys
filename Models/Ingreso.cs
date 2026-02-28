using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSys.Models
{
    public class Ingreso
    {
        public int Id { get; set; }
        public DateTime FechaIngreso { get; set; } = DateTime.Now;

        // [REFACCION]: Diagnóstico ahora es condicionalmente obligatorio (se valida en el controlador).
        [Column("DiagnosticoInicial")]
        [StringLength(2000)]
        public string? Diagnostico { get; set; }

        [StringLength(512, ErrorMessage = "La ruta de la foto no puede exceder los 512 caracteres.")]
        public string? FotoPath { get; set; }
        public DateTime? FechaEgreso { get; set; }
        
        [Required(ErrorMessage = "El estado es obligatorio.")]
        [StringLength(50, ErrorMessage = "El estado no puede exceder los 50 caracteres.")]
        public string Estado { get; set; } = "En revisión";

        [Required(ErrorMessage = "Debe seleccionar un vehículo.")]
        public int? VehiculoId { get; set; }
        public Vehiculo? Vehiculo { get; set; }

        // Snapshot del cliente en el momento del ingreso
        public int? ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        // [REFACCION]: Relación N:M con Servicios Fijos
        public ICollection<ServicioFijo> ServiciosFijos { get; set; } = new List<ServicioFijo>();

        public ICollection<FotoVehiculo>? Fotos { get; set; }

        [NotMapped]
        public int DiasEnTaller
        {
            get
            {
                var fechaFin = FechaEgreso ?? DateTime.Now;
                return (fechaFin - FechaIngreso).Days;
            }
        }

        [NotMapped]
        public string ColorSemaforo
        {
            get
            {
                if (FechaEgreso.HasValue || Estado == "Listo para entrega" || Estado == "Finalizado" || Estado == "Entregado")
                    return "success";
                
                if (Estado == "En reparación" || Estado == "En proceso")
                    return "warning";
                
                if (Estado == "En revisión")
                    return "danger";
                
                return "danger";
            }
        }

        [NotMapped]
        public string EstadoDescripcion
        {
            get
            {
                if (FechaEgreso.HasValue)
                    return "Entregado";
                
                return Estado switch
                {
                    "En revisión" => "En Revisión",
                    "En reparación" => "En Proceso",
                    "En proceso" => "En Proceso",
                    "Esperando repuestos" => "En Revisión",
                    "Listo para entrega" => "Finalizado",
                    "Finalizado" => "Finalizado",
                    "Entregado" => "Entregado",
                    _ => Estado
                };
            }
        }
    }
}