using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
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

        services.AddAuthorization();

        return services;
    }

    private static Task AddProviderClaim(OAuthCreatingTicketContext context, string provider)
    {
        context.Identity?.AddClaim(new Claim(ProviderClaimType, provider));
        return Task.CompletedTask;
    }
}
