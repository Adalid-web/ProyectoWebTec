using System.Security.Claims;
using AspNet.Security.OAuth.Dropbox;
using AspNet.Security.OAuth.Spotify;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace ProyectoTecWeb.Auth;

public static class AuthenticationExtensions
{
    public const string ProviderClaimType = "provider";

    public static IServiceCollection AddAppAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var authentication = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        });

        authentication.AddCookie(options =>
        {
            options.Cookie.Name = "ProyectoTecWeb.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.LoginPath = "/";
            options.AccessDeniedPath = "/";
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);

            // PRUEBA LOCAL: cada cookie nueva tiene ID revocable en PostgreSQL.
            options.Events.OnSigningIn = async context =>
            {
                if (context.Principal?.Identity is not ClaimsIdentity identity)
                    throw new InvalidOperationException("No se recibió una identidad válida.");

                var sessionId = Guid.NewGuid().ToString("N");
                identity.AddClaim(new Claim(LocalSingleSignOutStore.SessionIdClaim, sessionId));
                var store = context.HttpContext.RequestServices
                    .GetRequiredService<LocalSingleSignOutStore>();
                await store.RegisterAsync(context.Principal, sessionId, context.HttpContext.RequestAborted);
            };

            // El ID se consulta en la BD en cada petición, también en otras instancias.
            options.Events.OnValidatePrincipal = async context =>
            {
                var sessionId = context.Principal?
                    .FindFirst(LocalSingleSignOutStore.SessionIdClaim)?.Value;
                var store = context.HttpContext.RequestServices
                    .GetRequiredService<LocalSingleSignOutStore>();
                if (!await store.IsActiveAsync(sessionId, context.HttpContext.RequestAborted))
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
            };

            // La API debe responder 401/403, no redirigir al login cuando no hay sesión.
            options.Events.OnRedirectToLogin = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };

            options.Events.OnRedirectToAccessDenied = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };
        });

        var googleCredentials = AuthProviderCatalog.GetCredentials(configuration, "Google");
        if (googleCredentials is not null)
        {
            authentication.AddGoogle(options =>
            {
                options.ClientId = googleCredentials.ClientId;
                options.ClientSecret = googleCredentials.ClientSecret;
                options.CallbackPath = "/signin-google";
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.SaveTokens = false;
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.Events.OnCreatingTicket = context =>
                    AddProviderClaim(context, "Google");
            });
        }

        var discordCredentials = AuthProviderCatalog.GetCredentials(configuration, "Discord");
        if (discordCredentials is not null)
        {
            authentication.AddDiscord(options =>
            {
                options.ClientId = discordCredentials.ClientId;
                options.ClientSecret = discordCredentials.ClientSecret;
                options.CallbackPath = "/signin-discord";
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.SaveTokens = false;
                options.Scope.Add("identify");
                options.Scope.Add("email");
                options.Events.OnCreatingTicket = context =>
                    AddProviderClaim(context, "Discord");
            });
        }

        var dropboxCredentials = AuthProviderCatalog.GetCredentials(configuration, "Dropbox");
        if (dropboxCredentials is not null)
        {
            authentication.AddDropbox(options =>
            {
                options.ClientId = dropboxCredentials.ClientId;
                options.ClientSecret = dropboxCredentials.ClientSecret;
                options.CallbackPath = "/signin-dropbox";
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.SaveTokens = false;
                options.Events.OnCreatingTicket = context =>
                    AddProviderClaim(context, "Dropbox");
            });
        }

        var spotifyCredentials = AuthProviderCatalog.GetCredentials(configuration, "Spotify");
        if (spotifyCredentials is not null)
        {
            authentication.AddSpotify(options =>
            {
                options.ClientId = spotifyCredentials.ClientId;
                options.ClientSecret = spotifyCredentials.ClientSecret;
                options.CallbackPath = "/signin-spotify";
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.SaveTokens = false;
                options.Scope.Add("user-read-email");
                options.Events.OnCreatingTicket = context =>
                    AddProviderClaim(context, "Spotify");
            });
        }

        services.AddAuthorization();

        return services;
    }

    private static Task AddProviderClaim(OAuthCreatingTicketContext context, string provider)
    {
        context.Identity?.AddClaim(new Claim(ProviderClaimType, provider));
        return Task.CompletedTask;
    }
}