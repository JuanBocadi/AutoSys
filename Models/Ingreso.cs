using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSys.Models
{
    public class Ingreso
    {
        public int Id { get; set; }
        public DateTime FechaIngreso { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Debe ingresar un diagnóstico.")]
        [Column("DiagnosticoInicial")]
        [StringLength(2000)]
        public string Diagnostico { get; set; } = string.Empty;

        [StringLength(512, ErrorMessage = "La ruta de la foto no puede exceder los 512 caracteres.")]
        public string? FotoPath { get; set; }
        public DateTime? FechaEgreso { get; set; }
        
        [Required(ErrorMessage = "El estado es obligatorio.")]
        [StringLength(50, ErrorMessage = "El estado no puede exceder los 50 caracteres.")]
        public string Estado { get; set; } = "En revisión";

        [Required(ErrorMessage = "Debe seleccionar un vehículo.")]
        public int? VehiculoId { get; set; }
        public Vehiculo? Vehiculo { get; set; }
        public ICollection<FotoVehiculo>? Fotos { get; set; }

        // Propiedades calculadas para semaforización
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
                // Semaforización por estado de la reparación según avance del trabajo
                // Rojo: En revisión (recién ingresado, aún no comenzó)
                // Amarillo: En proceso/reparación (trabajo en marcha)
                // Verde: Finalizado/Entregado (trabajo completado)
                
                // Verde: Trabajo completado
                if (FechaEgreso.HasValue || Estado == "Listo para entrega" || Estado == "Finalizado" || Estado == "Entregado")
                    return "success"; // Verde - Finalizado/Entregado
                
                // Amarillo: Trabajo en marcha
                if (Estado == "En reparación" || Estado == "En proceso")
                    return "warning"; // Amarillo - En proceso
                
                // Rojo: Recién ingresado o esperando inicio
                if (Estado == "En revisión")
                    return "danger"; // Rojo - En revisión
                
                // Por defecto, si está esperando repuestos también es rojo (requiere acción)
                return "danger"; // Rojo - Esperando acción
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