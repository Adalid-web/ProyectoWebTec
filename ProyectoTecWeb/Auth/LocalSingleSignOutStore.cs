using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Npgsql;

namespace ProyectoTecWeb.Auth;

/// <summary>
/// Registro compartido de sesiones de Circle Rush. Revoca cookies entre equipos
/// e instancias del backend usando PostgreSQL; NO cierra sesiones de Twitch/Slack.
/// </summary>
public sealed class LocalSingleSignOutStore
{
    public const string SessionIdClaim = "circle_rush_sso_session_id";

    private readonly string _connectionString;

    public LocalSingleSignOutStore(IConfiguration configuration)
    {
        _connectionString = configuration["ConnectionStrings:DefaultConnection"]
            ?? throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection.");
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS circle_rush_sso_sessions (
                session_id text PRIMARY KEY,
                provider text NOT NULL,
                subject_id text NOT NULL,
                issued_at timestamptz NOT NULL DEFAULT now(),
                expires_at timestamptz NOT NULL,
                revoked_at timestamptz NULL
            );
            CREATE INDEX IF NOT EXISTS ix_circle_rush_sso_identity
                ON circle_rush_sso_sessions (provider, subject_id);
            """;
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static (string Provider, string Subject)? IdentityOf(ClaimsPrincipal principal)
    {
        var provider = principal.FindFirstValue(AuthenticationExtensions.ProviderClaimType);
        // Para proveedores OAuth/OIDC se prioriza el ID inmutable del proveedor.
        var subject = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub")
            ?? principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(subject))
            return null;
        return (provider, subject);
    }

    public async Task RegisterAsync(
        ClaimsPrincipal principal, string sessionId, CancellationToken cancellationToken = default)
    {
        var identity = IdentityOf(principal)
            ?? throw new InvalidOperationException("El proveedor no entregó una identidad válida para Single Sign-Out.");
        // La cookie de autenticación sigue teniendo su propio vencimiento de 8 horas.
        // Estos registros se conservan 30 días como límite superior de retención.
        const string sql = """
            INSERT INTO circle_rush_sso_sessions
                (session_id, provider, subject_id, expires_at)
            VALUES (@sid, @provider, @subject, @expires)
            ON CONFLICT (session_id) DO NOTHING;
            """;
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("sid", sessionId);
        command.Parameters.AddWithValue("provider", identity.Provider);
        command.Parameters.AddWithValue("subject", identity.Subject);
        command.Parameters.AddWithValue("expires", DateTime.UtcNow.AddDays(30));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> IsActiveAsync(string? sessionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId)) return false;
        const string sql = """
            SELECT EXISTS (
                SELECT 1 FROM circle_rush_sso_sessions
                WHERE session_id = @sid
                  AND revoked_at IS NULL
                  AND expires_at > now()
            );
            """;
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("sid", sessionId);
        return (bool)(await command.ExecuteScalarAsync(cancellationToken) ?? false);
    }

    public async Task RevokeOneAsync(string? sessionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId)) return;
        const string sql = """
            UPDATE circle_rush_sso_sessions
            SET revoked_at = now()
            WHERE session_id = @sid AND revoked_at IS NULL;
            """;
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("sid", sessionId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>Revoca todas las cookies de Circle Rush del MISMO proveedor + usuario.</summary>
    public async Task<int> RevokeAllAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        var identity = IdentityOf(principal)
            ?? throw new InvalidOperationException("La sesión no contiene identidad del proveedor.");
        const string sql = """
            UPDATE circle_rush_sso_sessions
            SET revoked_at = now()
            WHERE provider = @provider
              AND subject_id = @subject
              AND revoked_at IS NULL;
            """;
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("provider", identity.Provider);
        command.Parameters.AddWithValue("subject", identity.Subject);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
