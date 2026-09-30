using Microsoft.EntityFrameworkCore;
using ResilienciaNorte.Repository;
using ResilienciaNorte.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Inyección de DbContext (Capa Repository)
builder.Services.AddDbContext<ResilienciaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Inyección de la Capa de Servicios (Regla: El controlador solo consume servicios)
builder.Services.AddScoped<IIncidenteService, IncidenteService>();
builder.Services.AddScoped<IRecursoService, RecursoService>();

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

// Mapear Hubs y Endpoints después de Authorization
app.MapHub<ResilienciaNorte.Web.Hubs.EmergenciaHub>("/emergenciaHub");
app.MapStaticAssets();

// Redirigir la raíz hacia el panel de incidentes
app.MapGet("/", () => Results.Redirect("/Incidentes"));

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Incidentes}/{action=Index}/{id?}")
    .WithStaticAssets();

// Aplicar migraciones y Seeding automáticamente si la BD no existe
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ResilienciaDbContext>();
        context.Database.Migrate(); // Aplica cualquier migración pendiente
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al ejecutar migraciones y sembrado.");
    }
}

app.Run();