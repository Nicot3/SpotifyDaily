using Microsoft.Extensions.Options;
using SpotifyAPI.Web;
using SpotifyDaily.Worker.Exceptions;
using SpotifyDaily.Worker.Models;
using SpotifyDaily.Worker.Options;
using SpotifyDaily.Worker.Services.Contracts;

namespace SpotifyDaily.Worker.Services;

public class SpotifyClientService(IAppConfigService appConfigService, ILogger<SpotifyClientService> logger, IOptions<SpotifyOptions> spotifyOptions) : ISpotifyClientService
{
    private readonly SpotifyOptions _spotifyOptions = spotifyOptions.Value;
    private readonly AppConfig _appConfig = appConfigService.Current;
    private SpotifyClient? _client;

    private SpotifyClient? Client
    {
        get
        {
            return _client;
        }
        set
        {
            _client = value;
            OnClientConfigured?.Invoke();
        }
    }

    public event Action? OnClientConfigured;

    public async Task ConfigureNewClientAsync(string code, CancellationToken cancellationToken)
    {
        logger.LogInformation("Configuring new Spotify client");
        if (string.IsNullOrEmpty(code))
        {
            throw new ArgumentException("Authorization code cannot be null or empty.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(_spotifyOptions.ClientId) || string.IsNullOrWhiteSpace(_spotifyOptions.ClientSecret))
        {
            throw new InvalidOperationException("Spotify configuration is not set.");
        }

        var request = new AuthorizationCodeTokenRequest(_spotifyOptions.ClientId, _spotifyOptions.ClientSecret, code, new Uri(_spotifyOptions.RedirectURI));
        var response = await new OAuthClient().RequestToken(request, cancellationToken);

        await SaveTokensAsync(response.AccessToken, response.RefreshToken, response.CreatedAt.AddSeconds(response.ExpiresIn));

        Client = new SpotifyClient(response.AccessToken);

        OnClientConfigured?.Invoke();

        logger.LogInformation("Spotify client configured successfully.");
    }

    private async Task ConfigureExistingClientAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Configuring Spotify client using refresh token");

        if (string.IsNullOrWhiteSpace(_appConfig.RefreshToken))
        {
            throw new ClientTokenExpiredException($"Cannot configure Spotify client: Refresh token is missing or expired.");
        }

        if (string.IsNullOrWhiteSpace(_spotifyOptions.ClientId) ||string.IsNullOrWhiteSpace(_spotifyOptions.ClientSecret))
        {
            throw new ClientException("Spotify Client ID and Client Secret must be configured in SpotifyOptions.");
        }

        var refreshRequest = new AuthorizationCodeRefreshRequest(_spotifyOptions.ClientId, _spotifyOptions.ClientSecret, _appConfig.RefreshToken);

        var response = await new OAuthClient().RequestToken(refreshRequest, cancellationToken);

        await SaveTokensAsync(response.AccessToken, response.RefreshToken, response.CreatedAt.AddSeconds(response.ExpiresIn));

        Client = new SpotifyClient(response.AccessToken);

        OnClientConfigured?.Invoke();

        logger.LogInformation("Spotify client configured successfully using refresh token.");
    }

    private async Task SaveTokensAsync(string accessToken, string refreshToken, DateTime expiresAt)
    {
        var newAppConfig = appConfigService.Current;

        newAppConfig.Token = accessToken;
        newAppConfig.RefreshToken = refreshToken;
        newAppConfig.ExpireDate = expiresAt;

        await appConfigService.UpdateAsync(newAppConfig);

    }

    public async Task<SpotifyClient> GetClientAsync(string? code = null, CancellationToken cancellationToken = default)
    {
        if (Client != null)
        {
            return Client;
        }

        if (_appConfig.ExpireDate > DateTime.Now && !string.IsNullOrWhiteSpace(_appConfig.Token))
        {
            Client = new SpotifyClient(_appConfig.Token);
            return Client;
        }

        try
        {
            await ConfigureExistingClientAsync(cancellationToken);
        }
        catch (ClientTokenExpiredException e)
        {
            var url = GetLoginUri().ToString();
            logger.LogWarning(e, "Client token expired. Attempting to configure new client with provided code. Login using {url}", url);
        }

        return Client ?? throw new ClientException();
    }

    private Uri GetLoginUri()
    {
        if (string.IsNullOrWhiteSpace(_spotifyOptions.RedirectURI) || string.IsNullOrWhiteSpace(_spotifyOptions.ClientId))
        {
            throw new ClientException("Spotify Redirect URI and Client ID must be configured in SpotifyOptions.");
        }

        var loginRequest = new LoginRequest(new Uri(_spotifyOptions.RedirectURI), _spotifyOptions.ClientId, LoginRequest.ResponseType.Code)
        {
            Scope = [ Scopes.PlaylistModifyPrivate, Scopes.PlaylistModifyPublic, Scopes.PlaylistReadPrivate, Scopes.UserTopRead ]
        };

        return loginRequest.ToUri();
    }
}