using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using ProyectoTecWeb.Auth;
using ProyectoTecWeb.Data;

var builder = WebApplication.CreateBuilder(args);

// El puerto se lee de una variable de entorno para que el contenedor sea configurable.
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// La cadena de conexión se inyecta en tiempo de ejecución; nunca se hornea en la imagen.
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Falta la cadena de conexión. Define ConnectionStrings__DefaultConnection.");
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddAppAuthentication(builder.Configuration);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
        | ForwardedHeaders.XForwardedHost
        | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseForwardedHeaders();

ApplyMigrations(app);

app.UseSwagger();
app.UseSwaggerUI();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapAuthEndpoints();
app.MapHealthChecks("/health");
app.MapGet("/health/db", async (AppDbContext db) =>
{
    var canConnect = await db.Database.CanConnectAsync();
    return canConnect
        ? Results.Ok(new { status = "ok", database = "connected" })
        : Results.Json(
            new { status = "error", database = "disconnected" },
            statusCode: StatusCodes.Status503ServiceUnavailable);
});

app.Run();

static void ApplyMigrations(WebApplication app)
{
    const int maxAttempts = 10;

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
            app.Logger.LogInformation("Migraciones aplicadas correctamente.");
            return;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            app.Logger.LogWarning(
                ex,
                "PostgreSQL no está listo. Reintentando en 3 segundos ({Attempt}/{MaxAttempts})...",
                attempt,
                maxAttempts);
            Thread.Sleep(TimeSpan.FromSeconds(3));
        }
    }

    throw new InvalidOperationException("No se pudieron aplicar las migraciones.");
}