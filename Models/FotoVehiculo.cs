namespace AutoSys.Models
{
    public class FotoVehiculo
    {
        public int Id { get; set; }
        public int IngresoId { get; set; }
        public string Tipo { get; set; } = "Ingreso"; // Ingreso y Egreso
        public string RutaArchivo { get; set; } = string.Empty;
        public DateTime FechaCarga { get; set; } = DateTime.Now;
        public Ingreso Ingreso { get; set; } = null!;
    }
}


