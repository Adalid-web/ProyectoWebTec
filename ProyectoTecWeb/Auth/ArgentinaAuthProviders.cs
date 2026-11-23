using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace ProyectoTecWeb.Auth;

/// <summary>Extensión exclusiva de Argentina; reutiliza la cookie y los endpoints del líder.</summary>
public static class ArgentinaAuthProviders
{
    public static IServiceCollection AddArgentinaAuthProviders(
        this IServiceCollection services, IConfiguration configuration)
    {
        var authentication = services.AddAuthentication();

        var twitch = AuthProviderCatalog.GetCredentials(configuration, "Twitch");
        if (twitch is not null)
        {
            authentication.AddOAuth("Twitch", options =>
            {
                options.ClientId = twitch.ClientId;
                options.ClientSecret = twitch.ClientSecret;
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.CallbackPath = "/signin-twitch";
                options.AuthorizationEndpoint = "https://id.twitch.tv/oauth2/authorize";
                options.TokenEndpoint = "https://id.twitch.tv/oauth2/token";
                options.UserInformationEndpoint = "https://api.twitch.tv/helix/users";
                options.SaveTokens = false;
                options.UsePkce = false;
                options.Scope.Add("user:read:email");
                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "display_name");
                options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                options.Events.OnCreatingTicket = async context =>
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, options.UserInformationEndpoint);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                    request.Headers.Add("Client-Id", twitch.ClientId);
                    using var response = await context.Backchannel.SendAsync(
                        request, context.HttpContext.RequestAborted);
                    response.EnsureSuccessStatusCode();
                    await using var stream = await response.Content.ReadAsStreamAsync(
                        context.HttpContext.RequestAborted);
                    using var json = await JsonDocument.ParseAsync(
                        stream, cancellationToken: context.HttpContext.RequestAborted);
                    var users = json.RootElement.GetProperty("data");
                    if (users.GetArrayLength() < 1)
                        throw new InvalidOperationException("Twitch no devolvió la identidad del usuario.");
                    context.RunClaimActions(users[0]);
                    context.Identity?.AddClaim(new Claim(AuthenticationExtensions.ProviderClaimType, "Twitch"));
                };
            });
        }

        var slack = AuthProviderCatalog.GetCredentials(configuration, "Slack");
        if (slack is not null)
        {
            authentication.AddOpenIdConnect("Slack", options =>
            {
                options.ClientId = slack.ClientId;
                options.ClientSecret = slack.ClientSecret;
                options.Authority = "https://slack.com";
                options.MetadataAddress = "https://slack.com/.well-known/openid-configuration";
                options.CallbackPath = "/signin-slack";
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.ResponseType = "code";
                options.ResponseMode = "form_post";
                options.UsePkce = true;
                options.SaveTokens = false;
                options.GetClaimsFromUserInfoEndpoint = false;
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.TokenValidationParameters.NameClaimType = "name";
                options.Events.OnTokenValidated = context =>
                {
                    if (context.Principal?.Identity is ClaimsIdentity identity)
                        identity.AddClaim(new Claim(AuthenticationExtensions.ProviderClaimType, "Slack"));
                    return Task.CompletedTask;
                };
            });
        }
        return services;
    }
}
