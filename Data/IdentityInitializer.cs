using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace AutoSys.Data
{
    public class IdentityInitializer
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            string[] roles = { "Administrador", "Recepcionista", "Mecanico" };

            // Crear roles si no existen
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Crear usuario administrador si no existe
            string adminEmail = "admin@autosys.com";
            string adminPassword = "Admin123!";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var user = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Administrador");
                    Console.WriteLine("✅ Usuario administrador creado correctamente.");
                }
                else
                {
                    Console.WriteLine("⚠️ Error al crear usuario administrador:");
                    foreach (var error in result.Errors)
                        Console.WriteLine($"- {error.Description}");
                }
            }
            else
            {
                Console.WriteLine("ℹ️ El usuario administrador ya existe.");
            }
        }
    }
}
