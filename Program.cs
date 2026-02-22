using Microsoft.EntityFrameworkCore;
using AutoSys.Data;
using AutoSys.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Configurar antiforgery para admitir peticiones AJAX con header
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});

builder.Services.AddDbContext<AutoSysDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Política de contraseñas
    options.Password.RequireDigit           = true;
    options.Password.RequiredLength         = 8;
    options.Password.RequireUppercase       = true;
    options.Password.RequireLowercase       = true;
    options.Password.RequireNonAlphanumeric = true;

    // Bloqueo de cuenta: 5 intentos fallidos → 10 minutos
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(10);
    options.Lockout.AllowedForNewUsers      = true;
})
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
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Servicios y patrones de diseño
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPdfExportService, PdfExportService>();
builder.Services.AddScoped<AutoSys.Services.IPermissionService, AutoSys.Services.PermissionService>();
builder.Services.AddScoped<IBackupService, BackupService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// Servicio de backups automáticos programados
builder.Services.AddSingleton<BackupSchedulerService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<BackupSchedulerService>());

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
    var configuration = services.GetRequiredService<IConfiguration>();

    context.Database.Migrate();
    DbInitializer.Seed(context);
    await IdentityInitializer.SeedAsync(roleManager, userManager, configuration);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHttpsRedirection();   // forzar HTTPS en producción
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
