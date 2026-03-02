using Microsoft.EntityFrameworkCore;
using AutoSys.Data;
using AutoSys.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Compresión de respuestas (Gzip + Brotli)
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "text/css", "application/javascript", "text/html", "application/json", "image/svg+xml" });
});

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
builder.Services.AddScoped<IEmailService, EmailService>();

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

app.UseResponseCompression();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache estático por 7 días para imágenes, CSS, JS, fuentes
        var path = ctx.File.Name.ToLowerInvariant();
        if (path.EndsWith(".css") || path.EndsWith(".js") || path.EndsWith(".png") ||
            path.EndsWith(".jpg") || path.EndsWith(".jpeg") || path.EndsWith(".webp") ||
            path.EndsWith(".avif") || path.EndsWith(".gif") || path.EndsWith(".ico") ||
            path.EndsWith(".woff") || path.EndsWith(".woff2") || path.EndsWith(".ttf"))
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=604800, immutable");
        }
    }
});
app.UseRouting();

// Headers de seguridad
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Cross-Origin-Opener-Policy", "same-origin");
    await next();
});
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
