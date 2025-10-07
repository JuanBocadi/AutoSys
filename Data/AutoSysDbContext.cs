// Data/AutoSysDbContext.cs
using Microsoft.AspNetCore.Identity; // <-- AÑADE ESTE
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // <-- Y ESTE
using Microsoft.EntityFrameworkCore;
using AutoSys.Models;

namespace AutoSys.Data
{
    public class AutoSysDbContext : IdentityDbContext<IdentityUser>
    {
        public AutoSysDbContext(DbContextOptions<AutoSysDbContext> options) : base(options) { }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Vehiculo> Vehiculos { get; set; }
        public DbSet<Ingreso> Ingresos { get; set; }
        public DbSet<FotoVehiculo> FotosVehiculo { get; set; }
    }
}
