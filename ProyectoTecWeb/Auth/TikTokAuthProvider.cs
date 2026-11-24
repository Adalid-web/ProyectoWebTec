using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace ProyectoTecWeb.Auth;

public sealed class TikTokOAuthOptions : OAuthOptions
{
    public string RegisteredRedirectUri { get; set; } = "";
}

// TikTok uses client_key instead of the standard OAuth client_id.
// The ASP.NET Core handler still owns correlation cookies, protected state and sign-in.
public sealed class TikTokOAuthHandler(
    IOptionsMonitor<TikTokOAuthOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : OAuthHandler<TikTokOAuthOptions>(options, logger, encoder)
{
    protected override string BuildChallengeUrl(AuthenticationProperties properties, string redirectUri)
    {
        return QueryHelpers.AddQueryString(Options.AuthorizationEndpoint, new Dictionary<string, string?>
        {
            ["client_key"] = Options.ClientId,
            ["response_type"] = "code",
            ["scope"] = string.Join(",", Options.Scope),
            ["redirect_uri"] = Options.RegisteredRedirectUri,
            ["state"] = Options.StateDataFormat.Protect(properties)
        });
    }

    protected override async Task<OAuthTokenResponse> ExchangeCodeAsync(OAuthCodeExchangeContext context)
    {
        // Refuse a callback arriving on a different origin, before contacting TikTok.
        if (!string.Equals(context.RedirectUri, Options.RegisteredRedirectUri, StringComparison.Ordinal))
            return OAuthTokenResponse.Failed(new InvalidOperationException("Origen de retorno de TikTok incorrecto."));

        using var request = new HttpRequestMessage(HttpMethod.Post, Options.TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_key"] = Options.ClientId,
                ["client_secret"] = Options.ClientSecret,
                ["code"] = context.Code,
                ["grant_type"] = "authorization_code",
                ["redirect_uri"] = Options.RegisteredRedirectUri
            })
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        using var response = await Backchannel.SendAsync(request, Context.RequestAborted);
        if (!response.IsSuccessStatusCode)
            return OAuthTokenResponse.Failed(new InvalidOperationException("TikTok rechazó el intercambio del código."));
        var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Context.RequestAborted));
        if (!payload.RootElement.TryGetProperty("access_token", out var token) ||
            token.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(token.GetString()))
        {
            payload.Dispose();
            return OAuthTokenResponse.Failed(new InvalidOperationException("TikTok no entregó un token válido."));
        }
        return OAuthTokenResponse.Success(payload);
    }
}

public static class TikTokAuthProvider
{
    public static IServiceCollection AddTikTokAuthProvider(this IServiceCollection services, IConfiguration configuration)
    {
        var credentials = AuthProviderCatalog.GetCredentials(configuration, "TikTok");
        var redirectUri = AuthProviderCatalog.GetTikTokRedirectUri(configuration);
        if (credentials is null || redirectUri is null) return services;

        services.AddAuthentication().AddOAuth<TikTokOAuthOptions, TikTokOAuthHandler>("TikTok", "TikTok", options =>
        {
            options.ClientId = credentials.ClientId;
            options.ClientSecret = credentials.ClientSecret;
            options.RegisteredRedirectUri = redirectUri.AbsoluteUri;
            options.CallbackPath = "/signin-tiktok";
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.AuthorizationEndpoint = "https://www.tiktok.com/v2/auth/authorize/";
            options.TokenEndpoint = "https://open.tiktokapis.com/v2/oauth/token/";
            options.UserInformationEndpoint = "https://open.tiktokapis.com/v2/user/info/?fields=open_id,display_name";
            options.Scope.Add("user.info.basic");
            options.UsePkce = false; // Login Kit Web uses the confidential server-side code exchange.
            options.SaveTokens = false;
            options.Events.OnCreatingTicket = async context =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, options.UserInformationEndpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                using var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                response.EnsureSuccessStatusCode();
                using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(context.HttpContext.RequestAborted));
                var root = payload.RootElement;
                if (!root.TryGetProperty("error", out var error) || error.GetProperty("code").GetString() != "ok")
                    throw new InvalidOperationException("No se pudo consultar la identidad de TikTok.");
                var user = root.GetProperty("data").GetProperty("user");
                var id = user.GetProperty("open_id").GetString();
                if (string.IsNullOrWhiteSpace(id) || context.Identity is null)
                    throw new InvalidOperationException("TikTok no entregó un identificador válido.");
                var name = user.TryGetProperty("display_name", out var displayName) ? displayName.GetString() : null;
                context.Identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, id));
                context.Identity.AddClaim(new Claim(ClaimTypes.Name, string.IsNullOrWhiteSpace(name) ? "Usuario TikTok" : name));
                context.Identity.AddClaim(new Claim(AuthenticationExtensions.ProviderClaimType, "TikTok"));
            };
            options.Events.OnRemoteFailure = context =>
            {
                context.HandleResponse();
                context.Response.Redirect("/?auth_error=tiktok");
                return Task.CompletedTask;
            };
        });
        return services;
    }
}
