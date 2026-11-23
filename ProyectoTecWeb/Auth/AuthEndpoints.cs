using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ProyectoTecWeb.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/auth/providers", (IConfiguration configuration) =>
            Results.Ok(AuthProviderCatalog.GetAvailability(configuration)));

        endpoints.MapGet("/api/auth/me", (ClaimsPrincipal user) =>
        {
            if (user.Identity?.IsAuthenticated != true)
            {
                return Results.Unauthorized();
            }

            var id = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var name = user.Identity.Name
                ?? user.FindFirstValue(ClaimTypes.GivenName)
                ?? user.FindFirstValue(ClaimTypes.Email)
                ?? "Usuario";
            var email = user.FindFirstValue(ClaimTypes.Email);
            var provider = user.FindFirstValue(AuthenticationExtensions.ProviderClaimType)
                ?? "Cookie";

            return Results.Ok(new
            {
                id,
                name,
                email,
                provider
            });
        }).RequireAuthorization();

        endpoints.MapGet("/login/google", (IConfiguration configuration) =>
            ChallengeProvider(configuration, "Google"));

        endpoints.MapGet("/login/discord", (IConfiguration configuration) =>
            ChallengeProvider(configuration, "Discord"));

        endpoints.MapGet("/login/dropbox", (IConfiguration configuration) =>
            ChallengeProvider(configuration, "Dropbox"));

        endpoints.MapGet("/login/spotify", (IConfiguration configuration) =>
            ChallengeProvider(configuration, "Spotify"));

        endpoints.MapGet("/login/twitch", (IConfiguration configuration) =>
            ChallengeProvider(configuration, "Twitch"));

        endpoints.MapGet("/login/slack", (IConfiguration configuration) =>
            ChallengeProvider(configuration, "Slack"));

        endpoints.MapPost("/logout", async (HttpContext context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/");
        });

        return endpoints;
    }

    private static IResult ChallengeProvider(IConfiguration configuration, string scheme)
    {
        if (!AuthProviderCatalog.IsConfigured(configuration, scheme))
        {
            return Results.Problem(
                title: "Proveedor no configurado",
                detail: $"Agrega las credenciales de {scheme} en .env o en user-secrets.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        return Results.Challenge(
            new AuthenticationProperties { RedirectUri = "/" },
            [scheme]);
    }
}