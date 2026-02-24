using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace AutoSys.Data
{
    public class IdentityInitializer
    {
        public static async Task SeedAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager,
            IConfiguration configuration)
        {
            string[] roles = { "Administrador", "Recepcionista", "Mecanico" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Leer credenciales desde configuración (nunca hardcodeadas)
            string adminEmail    = configuration["AdminSeed:Email"]    ?? "admin@autosys.com";
            string adminUsername = configuration["AdminSeed:Username"] ?? "admin";
            string adminPassword = configuration["AdminSeed:Password"] ?? "Admin123!";

            // Migrar username antiguo si es necesario
            var oldAdminUser = await userManager.FindByNameAsync(adminEmail);
            if (oldAdminUser != null && oldAdminUser.UserName != adminUsername)
            {
                oldAdminUser.UserName = adminUsername;
                oldAdminUser.NormalizedUserName = adminUsername.ToUpper();
                var updateResult = await userManager.UpdateAsync(oldAdminUser);
                if (updateResult.Succeeded)
                    Console.WriteLine("[AutoSys] Usuario administrador actualizado.");
                return;
            }

            var adminByEmail = await userManager.FindByEmailAsync(adminEmail);
            if (adminByEmail != null && adminByEmail.UserName != adminUsername)
            {
                adminByEmail.UserName = adminUsername;
                adminByEmail.NormalizedUserName = adminUsername.ToUpper();
                var updateResult = await userManager.UpdateAsync(adminByEmail);
                if (updateResult.Succeeded)
                    Console.WriteLine("[AutoSys] Usuario administrador actualizado.");
                return;
            }

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
                    Console.WriteLine("[AutoSys] Usuario administrador creado.");
                }
                else
                {
                    Console.WriteLine("[AutoSys] Error al crear usuario administrador:");
                    foreach (var error in result.Errors)
                        Console.WriteLine($"  - {error.Description}");
                }
            }
            else if (adminUser.Email != adminEmail)
            {
                // Actualizar el email si cambió en la configuración
                adminUser.Email = adminEmail;
                adminUser.NormalizedEmail = adminEmail.ToUpper();
                adminUser.EmailConfirmed = true;
                await userManager.UpdateAsync(adminUser);
                Console.WriteLine($"[AutoSys] Email del administrador actualizado a: {adminEmail}");
            }
        }
    }
}

