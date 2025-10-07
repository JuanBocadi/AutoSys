namespace AutoSys.Models
{
    public class Vehiculo
    {
        public int Id { get; set; }
        public string Patente { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;
        public ICollection<Ingreso> Ingresos { get; set; } = new List<Ingreso>();
    }
}
