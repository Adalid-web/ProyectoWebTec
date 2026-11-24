using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using ProyectoTecWeb.Auth;
using ProyectoTecWeb.Data;

var builder = WebApplication.CreateBuilder(args);

// El puerto se lee de una variable de entorno para que el contenedor sea configurable.
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
var httpsCertPath = Environment.GetEnvironmentVariable("HTTPS_CERT_PATH");
if (!string.IsNullOrWhiteSpace(httpsCertPath))
{
    var certPassword = Environment.GetEnvironmentVariable("HTTPS_CERT_PASSWORD")
        ?? throw new InvalidOperationException("Falta HTTPS_CERT_PASSWORD.");
    builder.WebHost.ConfigureKestrel(server =>
    {
        server.ListenAnyIP(int.Parse(port));
        server.ListenAnyIP(8443, listen => listen.UseHttps(httpsCertPath, certPassword));
    });
}
else
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}
var keyDirectory = Environment.GetEnvironmentVariable("AUTH_KEYS_PATH");
if (!string.IsNullOrWhiteSpace(keyDirectory))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keyDirectory));
}

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
builder.Services.AddArgentinaAuthProviders(builder.Configuration);

// Nginx termina HTTPS y reenvia Host/Proto. Sin estas cabeceras, los callbacks
// OAuth se generarian con http://api:8080 en lugar de la URL publica.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
        | ForwardedHeaders.XForwardedHost
        | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Proyecto Final, punto 2: autorizacion real de los reportes.
// El boton del frontend no sustituye esta comprobacion del backend.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ReporteBolivia", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim(AuthenticationExtensions.ProviderClaimType, "Google", "Discord"));

    options.AddPolicy("ReportePeru", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim(AuthenticationExtensions.ProviderClaimType, "Dropbox", "Spotify"));

    options.AddPolicy("ReporteArgentina", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim(AuthenticationExtensions.ProviderClaimType, "Twitch", "Slack"));
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