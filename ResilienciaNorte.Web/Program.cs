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

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
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
    var db = scope.ServiceProvider.GetRequiredService<ResilienciaDbContext>();
    // Si la BD local no existe, EF Core la crea y ejecuta el Seeding de Trujillo al instante
    db.Database.Migrate();
}

app.Run();