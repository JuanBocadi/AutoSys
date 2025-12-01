using Microsoft.EntityFrameworkCore;
using AutoSys.Data;
using AutoSys.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AutoSysDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AutoSysDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
    options.LoginPath = "/Account/Login";
    options.SlidingExpiration = true; 
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PuedeGestionarIngresos", policy => policy.RequireRole("Administrador", "Recepcionista"));
    options.AddPolicy("PuedeEditarUsuarios", policy => policy.RequireRole("Administrador"));
    options.AddPolicy("PuedeVerReportes", policy => policy.RequireRole("Administrador"));
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Servicios y patrones de diseño
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IFileStorage, LocalFileStorage>();

// Patrón Observer: Registrar sujeto y observadores
builder.Services.AddSingleton<AutoSys.Patterns.Observer.EventSubject>();

var app = builder.Build();

// INICIALIZAR BD Y ROLES
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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
