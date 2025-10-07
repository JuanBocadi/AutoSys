using Microsoft.EntityFrameworkCore;
using AutoSys.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// =======================================================
// CONFIGURACIÓN DE SERVICIOS
// =======================================================

// Permite usar controladores y vistas Razor
builder.Services.AddControllersWithViews();

// 1. Configuración del DbContext (conexión a la base de datos)
builder.Services.AddDbContext<AutoSysDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Configurar Identity (usuarios y roles)
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AutoSysDbContext>()
    .AddDefaultTokenProviders();

// NUEVO: Configurar el comportamiento de la cookie de sesión
builder.Services.ConfigureApplicationCookie(options =>
{
    // Tiempo de vida de la cookie de sesión
    options.ExpireTimeSpan = TimeSpan.FromMinutes(15);

    // Ruta a la que se redirige si el usuario no está logueado
    options.LoginPath = "/Account/Login";

    // Si está en 'true', el tiempo de expiración se renueva con cada petición.
    // Esto es lo que implementa el "cierre por inactividad".
    options.SlidingExpiration = true; 
});

// 3. Configurar política de autorización global (requiere usuario logueado por defecto)
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// =======================================================
// INICIALIZAR BASE DE DATOS Y ROLES/USUARIO ADMIN
// =======================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AutoSysDbContext>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    // Aplicar migraciones pendientes
    context.Database.Migrate();

    // Poblar datos base (clientes y vehículos)
    DbInitializer.Seed(context);

    // Poblar datos de Identity (roles + usuario admin)
    await IdentityInitializer.SeedAsync(roleManager, userManager);
}

// =======================================================
// CONFIGURACIÓN DEL PIPELINE DE LA APLICACIÓN
// =======================================================

// Muestra página de error en caso de fallas
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

// Habilitar autenticación y autorización (¡orden importante!)
app.UseAuthentication();
app.UseAuthorization();

// Ruta por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
