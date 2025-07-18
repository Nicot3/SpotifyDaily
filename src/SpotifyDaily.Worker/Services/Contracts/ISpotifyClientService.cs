
using SpotifyAPI.Web;

namespace SpotifyDaily.Worker.Services.Contracts;

public interface ISpotifyClientService
{
    Task ConfigureNewClientAsync(string code, CancellationToken cancellationToken);
    Task<SpotifyClient> GetClientAsync(string? code = null, CancellationToken cancellationToken = default);
}