namespace ProyectoTecWeb.Auth;

public sealed record AuthProviderInfo(string Id, string Name, string Scheme, string Icon);

// Información pública para el frontend. Nunca contiene ClientId ni ClientSecret.
public sealed record AuthProviderAvailability(
    string Id, string Name, string Scheme, string Icon, bool Configured);
