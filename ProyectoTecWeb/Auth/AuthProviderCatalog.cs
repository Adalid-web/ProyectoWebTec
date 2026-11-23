namespace ProyectoTecWeb.Auth;

public sealed record ProviderCredentials(string ClientId, string ClientSecret);

public static class AuthProviderCatalog
{
    // Los compañeros agregan aquí sus proveedores cuando integren sus ramas.
    private static readonly AuthProviderInfo[] Providers =
    [
        new("google", "Google", "Google", "G"),
        new("discord", "Discord", "Discord", "◈"),
        new("twitch", "Twitch", "Twitch", "T"),
        new("slack", "Slack", "Slack", "S")
    ];

    public static IReadOnlyList<AuthProviderInfo> GetConfigured(IConfiguration configuration) =>
        Providers
            .Where(provider => IsConfigured(configuration, provider.Scheme))
            .ToList();

    public static bool IsConfigured(IConfiguration configuration, string scheme) =>
        GetCredentials(configuration, scheme) is not null;

    public static ProviderCredentials? GetCredentials(IConfiguration configuration, string scheme)
    {
        var section = configuration.GetSection($"Authentication:{scheme}");
        var clientId = section["ClientId"];
        var clientSecret = section["ClientSecret"];

        return string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret)
            ? null
            : new ProviderCredentials(clientId, clientSecret);
    }
}
