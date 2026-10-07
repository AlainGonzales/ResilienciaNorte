using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ResilienciaNorte.Domain;
using ResilienciaNorte.Repository;
using ResilienciaNorte.Service;
using System.Globalization;

// Configurar zona horaria global para Perú (UTC-5)
var zonaHoraPerú = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
TimeZoneInfo.ClearCachedData();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Inyección de DbContext (Capa Repository)
builder.Services.AddDbContext<ResilienciaDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    // Previene excepciones por diferencias menores de snapshot entre máquinas de desarrollo
    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// Configuración de ASP.NET Core Identity
builder.Services.AddIdentity<UsuarioAplicacion, IdentityRole>(options =>
{
    // Reglas de contraseña para entorno de desarrollo y pruebas
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    // Bloqueo temporal por intentos fallidos
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;

    // Unicidad
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ResilienciaDbContext>()
.AddDefaultTokenProviders();

// Configuración de redirección de cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Home/Index"; // Redirige a la pantalla principal con el modal de login
    options.AccessDeniedPath = "/Incidentes/Portal"; // Redirige al portal del ciudadano si intenta acceder sin permisos
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

// Inyección de la Capa de Servicios
builder.Services.AddScoped<IIncidenteService, IncidenteService>();
builder.Services.AddScoped<IRecursoService, RecursoService>();

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();
app.UseRouting();

// CRUCIAL: Authentication DEBE ir antes de Authorization
app.UseAuthentication();
app.UseAuthorization();

// Mapear Hubs de SignalR
app.MapHub<ResilienciaNorte.Web.Hubs.EmergenciaHub>("/emergenciaHub");
app.MapStaticAssets();

// La ruta raíz '/' ahora conduce a la vista pública Home/Index
app.MapGet("/", () => Results.Redirect("/Home/Index"));

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Aplicar migraciones y Seeding automáticamente si la BD no existe
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ResilienciaDbContext>();
        context.Database.Migrate(); // Aplica cualquier migración pendiente

        // Sembrar roles y usuarios de prueba (Admin y Logística)
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<UsuarioAplicacion>>();
        await DataSeeder.InicializarRolesYUsuariosAsync(roleManager, userManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al ejecutar migraciones y sembrado.");
    }
}

app.Run();