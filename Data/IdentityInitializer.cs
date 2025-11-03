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

            // Crear o actualizar usuario administrador
            string adminEmail = "admin@autosys.com";
            string adminUsername = "admin";
            string adminPassword = "Admin123!";

            // Buscar si existe un admin con el email como username (sistema antiguo)
            var oldAdminUser = await userManager.FindByNameAsync(adminEmail);
            if (oldAdminUser != null && oldAdminUser.UserName != adminUsername)
            {
                // Actualizar el username del admin antiguo
                Console.WriteLine($"🔄 Actualizando usuario admin de '{oldAdminUser.UserName}' a '{adminUsername}'...");
                oldAdminUser.UserName = adminUsername;
                oldAdminUser.NormalizedUserName = adminUsername.ToUpper();
                var updateResult = await userManager.UpdateAsync(oldAdminUser);
                if (updateResult.Succeeded)
                {
                    Console.WriteLine("✅ Usuario administrador actualizado correctamente.");
                    Console.WriteLine($"   Nuevo Username: {adminUsername}");
                }
                else
                {
                    Console.WriteLine("⚠️ Error al actualizar usuario administrador:");
                    foreach (var error in updateResult.Errors)
                        Console.WriteLine($"   - {error.Description}");
                }
                return;
            }

            // También buscar por email por si tiene un username diferente
            var adminByEmail = await userManager.FindByEmailAsync(adminEmail);
            if (adminByEmail != null && adminByEmail.UserName != adminUsername)
            {
                Console.WriteLine($"🔄 Actualizando usuario admin (encontrado por email) de '{adminByEmail.UserName}' a '{adminUsername}'...");
                adminByEmail.UserName = adminUsername;
                adminByEmail.NormalizedUserName = adminUsername.ToUpper();
                var updateResult = await userManager.UpdateAsync(adminByEmail);
                if (updateResult.Succeeded)
                {
                    Console.WriteLine("✅ Usuario administrador actualizado correctamente.");
                    Console.WriteLine($"   Nuevo Username: {adminUsername}");
                }
                else
                {
                    Console.WriteLine("⚠️ Error al actualizar usuario administrador:");
                    foreach (var error in updateResult.Errors)
                        Console.WriteLine($"   - {error.Description}");
                }
                return;
            }

            // Buscar si ya existe con el nuevo username
            var adminUser = await userManager.FindByNameAsync(adminUsername);
            if (adminUser == null)
            {
                var user = new IdentityUser
                {
                    UserName = adminUsername,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Administrador");
                    Console.WriteLine("✅ Usuario administrador creado correctamente.");
                    Console.WriteLine($"   Username: {adminUsername}");
                    Console.WriteLine($"   Password: {adminPassword}");
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
