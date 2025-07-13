namespace SpotifyDaily.Worker.Services.Contracts;

public interface IPlaylistService
{
    Task UpdateDailyPlaylistAsync(CancellationToken cancellationToken = default);
}
