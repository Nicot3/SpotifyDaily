
using SpotifyAPI.Web;

namespace SpotifyDaily.Worker.Services.Contracts;

public interface ISpotifyClientService
{
    Task ConfigureNewClientAsync(string code, CancellationToken cancellationToken);
    Task<ISpotifyClient> GetClientAsync(string? code = null, CancellationToken cancellationToken = default);
}