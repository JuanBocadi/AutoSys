using Microsoft.EntityFrameworkCore;
using AutoSys.Data;
using AutoSys.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Deja usar controladores y vistas Razor
builder.Services.AddControllersWithViews();

// Configuración del DbContext (conexión a la base de datos)
builder.Services.AddDbContext<AutoSysDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar Identity (usuarios y roles)
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AutoSysDbContext>()
    .AddDefaultTokenProviders();

// Configurar el comportamiento de la cookie de sesión
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
    options.LoginPath = "/Account/Login";
    options.SlidingExpiration = true; 
});

// Configurar política de autorización (usuario logueado por defecto)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PuedeGestionarIngresos", policy => policy.RequireRole("Administrador", "Recepcionista"));
    options.AddPolicy("PuedeEditarUsuarios", policy => policy.RequireRole("Administrador"));
    options.AddPolicy("PuedeVerReportes", policy => policy.RequireRole("Administrador"));
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Servicios de infraestructura
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IFileStorage, LocalFileStorage>();

var app = builder.Build();

// INICIALIZAR BASE DE DATOS Y ROLES/USUARIO ADMIN
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AutoSysDbContext>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    context.Database.Migrate();
    DbInitializer.Seed(context);
    await IdentityInitializer.SeedAsync(roleManager, userManager);
}

// Muestra página de error en caso de errores
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Ruta por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
