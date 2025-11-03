using AutoSys.Models;
using System.Linq;

namespace AutoSys.Data
{
    public class DbInitializer
    {
        public static void Seed(AutoSysDbContext context)
        {
            if (!context.Clientes.Any())
            {
                var cliente1 = new Cliente { Nombre = "Juan", Apellido = "Pérez", Telefono = "123456789", DNI = "20123456", Email = "juan.perez@example.com", FechaRegistro = DateTime.Now };
                var cliente2 = new Cliente { Nombre = "Ana", Apellido = "Gómez", Telefono = "987654321", DNI = "20987654", Email = "ana.gomez@example.com", FechaRegistro = DateTime.Now };
                var cliente3 = new Cliente { Nombre = "Carlos", Apellido = "Rodríguez", Telefono = "555123456", DNI = "22111222", Email = "carlos.rodriguez@example.com", FechaRegistro = DateTime.Now };
                var cliente4 = new Cliente { Nombre = "Laura", Apellido = "Martínez", Telefono = "456789123", DNI = "23333444", Email = "laura.martinez@example.com", FechaRegistro = DateTime.Now };
                var cliente5 = new Cliente { Nombre = "María", Apellido = "López", Telefono = "321654987", DNI = "24555666", Email = "maria.lopez@example.com", FechaRegistro = DateTime.Now };

                context.Clientes.AddRange(cliente1, cliente2, cliente3, cliente4, cliente5);
                context.SaveChanges();

                var vehiculo1 = new Vehiculo { Patente = "ABC123", Marca = "Ford", Modelo = "Fiesta", ClienteId = cliente1.Id };
                var vehiculo2 = new Vehiculo { Patente = "XYZ789", Marca = "Chevrolet", Modelo = "Corsa", ClienteId = cliente2.Id };
                var vehiculo3 = new Vehiculo { Patente = "LMN456", Marca = "Peugeot", Modelo = "208", ClienteId = cliente3.Id };
                var vehiculo4 = new Vehiculo { Patente = "DEF321", Marca = "Volkswagen", Modelo = "Gol", ClienteId = cliente4.Id };
                var vehiculo5 = new Vehiculo { Patente = "GHI654", Marca = "Toyota", Modelo = "Yaris", ClienteId = cliente5.Id };

                context.Vehiculos.AddRange(vehiculo1, vehiculo2, vehiculo3, vehiculo4, vehiculo5);
                context.SaveChanges();

                // Seed de Ingresos con diferentes estados para demostrar semaforización
                // Rojo: En revisión
                var ingreso1 = new Ingreso
                {
                    VehiculoId = vehiculo1.Id,
                    FechaIngreso = DateTime.Now.AddDays(-1),
                    Diagnostico = "Revisión general. El cliente reporta ruidos en el motor al acelerar.",
                    Estado = "En revisión",
                    FechaEgreso = null
                };

                // Amarillo: En proceso
                var ingreso2 = new Ingreso
                {
                    VehiculoId = vehiculo4.Id,
                    FechaIngreso = DateTime.Now.AddDays(-3),
                    Diagnostico = "Cambio de pastillas de freno y discos. Sistema de frenos desgastado.",
                    Estado = "En proceso",
                    FechaEgreso = null
                };

                var ingreso3 = new Ingreso
                {
                    VehiculoId = vehiculo5.Id,
                    FechaIngreso = DateTime.Now.AddDays(-5),
                    Diagnostico = "Reparación de sistema eléctrico. Alternador defectuoso.",
                    Estado = "En reparación",
                    FechaEgreso = null
                };

                // Verde: Finalizado pero AÚN en taller (sin fecha de egreso)
                var ingreso4 = new Ingreso
                {
                    VehiculoId = vehiculo3.Id,
                    FechaIngreso = DateTime.Now.AddDays(-7),
                    Diagnostico = "Service de 10.000 km. Cambio de aceite, filtros y revisión completa.",
                    Estado = "Finalizado",
                    FechaEgreso = null // Aún en taller, esperando ser entregado
                };

                // Verde: Entregado (fuera del taller con fecha de egreso)
                var ingreso5 = new Ingreso
                {
                    VehiculoId = vehiculo2.Id,
                    FechaIngreso = DateTime.Now.AddDays(-10),
                    Diagnostico = "Cambio de neumáticos y alineación. Balanceo de ruedas.",
                    Estado = "Entregado",
                    FechaEgreso = DateTime.Now.AddDays(-2) // Ya fue retirado por el cliente
                };

                context.Ingresos.AddRange(ingreso1, ingreso2, ingreso3, ingreso4, ingreso5);
                context.SaveChanges();
            }

            // Seed de Stock con diferentes niveles de semaforización
            if (!context.Stock.Any())
            {
                // Items con stock SUFICIENTE (Verde)
                var stock1 = new Stock 
                { 
                    Nombre = "Aceite 10W40", 
                    Descripcion = "Aceite para motor sintético 10W40", 
                    Cantidad = 50, 
                    StockMinimo = 10, 
                    Unidad = "Litros",
                    PrecioUnitario = 12.50m,
                    FechaActualizacion = DateTime.Now 
                };

                var stock2 = new Stock 
                { 
                    Nombre = "Filtro de Aceite", 
                    Descripcion = "Filtro de aceite universal", 
                    Cantidad = 30, 
                    StockMinimo = 8, 
                    Unidad = "Unidades",
                    PrecioUnitario = 8.00m,
                    FechaActualizacion = DateTime.Now 
                };

                // Items con stock BAJO (Amarillo)
                var stock3 = new Stock 
                { 
                    Nombre = "Pastillas de Freno", 
                    Descripcion = "Pastillas de freno delanteras cerámicas", 
                    Cantidad = 12, 
                    StockMinimo = 10, 
                    Unidad = "Juegos",
                    PrecioUnitario = 45.00m,
                    FechaActualizacion = DateTime.Now 
                };

                var stock4 = new Stock 
                { 
                    Nombre = "Bujías", 
                    Descripcion = "Bujías de encendido platino", 
                    Cantidad = 15, 
                    StockMinimo = 12, 
                    Unidad = "Unidades",
                    PrecioUnitario = 6.50m,
                    FechaActualizacion = DateTime.Now 
                };

                // Items con stock CRÍTICO (Rojo)
                var stock5 = new Stock 
                { 
                    Nombre = "Correa de Distribución", 
                    Descripcion = "Correa de distribución reforzada", 
                    Cantidad = 3, 
                    StockMinimo = 5, 
                    Unidad = "Unidades",
                    PrecioUnitario = 85.00m,
                    FechaActualizacion = DateTime.Now 
                };

                var stock6 = new Stock 
                { 
                    Nombre = "Refrigerante", 
                    Descripcion = "Líquido refrigerante concentrado", 
                    Cantidad = 4, 
                    StockMinimo = 10, 
                    Unidad = "Litros",
                    PrecioUnitario = 15.00m,
                    FechaActualizacion = DateTime.Now 
                };

                var stock7 = new Stock 
                { 
                    Nombre = "Batería 12V", 
                    Descripcion = "Batería 12V 60Ah libre de mantenimiento", 
                    Cantidad = 2, 
                    StockMinimo = 4, 
                    Unidad = "Unidades",
                    PrecioUnitario = 120.00m,
                    FechaActualizacion = DateTime.Now 
                };

                context.Stock.AddRange(stock1, stock2, stock3, stock4, stock5, stock6, stock7);
                context.SaveChanges();
            }
        }
    }
}
