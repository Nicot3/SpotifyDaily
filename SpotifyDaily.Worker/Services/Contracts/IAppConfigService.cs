using SpotifyDaily.Worker.Models;

namespace SpotifyDaily.Worker.Services.Contracts;

public interface IAppConfigService
{
    AppConfig Current { get; set; }
    event Action<AppConfig>? OnChange;

    Task UpdateAsync(AppConfig newSettings);
}