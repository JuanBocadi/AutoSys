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
        public DbSet<Stock> Stock { get; set; }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<DetalleFactura> DetallesFactura { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Clientes
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Apellido).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Telefono).HasMaxLength(50);
                entity.Property(e => e.DNI).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(256);

                entity.HasIndex(e => e.DNI).IsUnique().HasFilter("[DNI] IS NOT NULL");
                entity.HasIndex(e => e.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
            });

            // Vehiculos
            modelBuilder.Entity<Vehiculo>(entity =>
            {
                entity.Property(e => e.Patente).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Marca).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Modelo).HasMaxLength(100).IsRequired();

                entity.HasIndex(e => e.Patente).IsUnique();

                entity.HasOne(v => v.Cliente)
                    .WithMany(c => c.Vehiculos)
                    .HasForeignKey(v => v.ClienteId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Ingresos
            modelBuilder.Entity<Ingreso>(entity =>
            {
                entity.Property(e => e.Diagnostico).HasColumnName("DiagnosticoInicial").HasMaxLength(2000).IsRequired();
                entity.Property(e => e.Estado).HasMaxLength(50).IsRequired();
                entity.Property(e => e.FotoPath).HasMaxLength(512);

                entity.HasOne(i => i.Vehiculo)
                    .WithMany(v => v.Ingresos)
                    .HasForeignKey(i => i.VehiculoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.VehiculoId, e.FechaIngreso });
            });

            // FotosVehiculo
            modelBuilder.Entity<FotoVehiculo>(entity =>
            {
                entity.Property(e => e.Tipo).HasMaxLength(20).IsRequired();
                entity.Property(e => e.RutaArchivo).HasMaxLength(512).IsRequired();

                entity.HasOne(f => f.Ingreso)
                    .WithMany(i => i.Fotos!)
                    .HasForeignKey(f => f.IngresoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Stock
            modelBuilder.Entity<Stock>(entity =>
            {
                entity.Property(e => e.Nombre).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Descripcion).HasMaxLength(1000);
                entity.Property(e => e.Unidad).HasMaxLength(50);
                entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(18,2)");

                entity.HasIndex(e => e.Nombre).IsUnique();
            });

            // Facturas
            modelBuilder.Entity<Factura>(entity =>
            {
                entity.Property(e => e.NumeroFactura).HasMaxLength(50).IsRequired();
                entity.Property(e => e.MetodoPago).HasMaxLength(20);
                entity.Property(e => e.Estado).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.IVA).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Total).HasColumnType("decimal(18,2)");

                entity.HasIndex(e => e.NumeroFactura).IsUnique();

                entity.HasOne(f => f.Ingreso)
                    .WithMany()
                    .HasForeignKey(f => f.IngresoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.Cliente)
                    .WithMany()
                    .HasForeignKey(f => f.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // UserPermissions
            modelBuilder.Entity<UserPermission>(entity =>
            {
                entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();
                entity.Property(e => e.ModificadoPor).HasMaxLength(256);
                entity.HasIndex(e => e.UserId).IsUnique();
            });

            // RolePermissions
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.Property(e => e.RolNombre).HasMaxLength(50).IsRequired();
                entity.Property(e => e.ModificadoPor).HasMaxLength(256);
                entity.HasIndex(e => e.RolNombre).IsUnique();
            });

            // DetallesFactura
            modelBuilder.Entity<DetalleFactura>(entity =>
            {
                entity.Property(e => e.Descripcion).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Tipo).HasMaxLength(20);
                entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");

                entity.HasOne(d => d.Factura)
                    .WithMany(f => f.Detalles)
                    .HasForeignKey(d => d.FacturaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // AuditLogs
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.Property(e => e.Usuario).HasMaxLength(256).IsRequired();
                entity.Property(e => e.Rol).HasMaxLength(50);
                entity.Property(e => e.Categoria).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Accion).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Descripcion).HasMaxLength(500).IsRequired();
                entity.Property(e => e.EntidadNombre).HasMaxLength(200);
                entity.Property(e => e.DireccionIP).HasMaxLength(50);

                entity.HasIndex(e => e.Fecha);
                entity.HasIndex(e => e.Categoria);
                entity.HasIndex(e => e.Usuario);
            });
        }
    }
}
