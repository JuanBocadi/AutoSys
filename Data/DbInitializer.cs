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
            }
        }
    }
}
