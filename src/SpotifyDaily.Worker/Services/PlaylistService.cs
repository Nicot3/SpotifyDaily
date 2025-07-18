using Microsoft.Extensions.Options;
using SpotifyAPI.Web;
using SpotifyDaily.Worker.Exceptions;
using SpotifyDaily.Worker.Options;
using SpotifyDaily.Worker.Services.Contracts;

namespace SpotifyDaily.Worker.Services;

public class PlaylistService(ILogger<PlaylistService> logger, IOptions<SpotifyOptions> spotifyOptions, ISpotifyClientService spotifyClientService) : IPlaylistService
{

    public async Task UpdateDailyPlaylistAsync(CancellationToken cancellationToken = default)
    {
        var client = await spotifyClientService.GetClientAsync(cancellationToken: cancellationToken);
        if (client == null)
        {
            throw new PlaylistServiceException("Spotify client is not configured. Please call ConfigureClientAsync first.");
        }

        SpotifyOptions spotifyConfig = spotifyOptions.Value;

        if (string.IsNullOrWhiteSpace(spotifyConfig.PlaylistId))
        {
            throw new PlaylistServiceException("Playlist ID must be configured in SpotifyConfig.");
        }

        logger.LogInformation("Updating daily playlist...");

        await RemoveCurrentTracksAsync(client, spotifyConfig.PlaylistId, cancellationToken);

        IEnumerable<FullTrack> topTracks = await GetTopTracksAsync(client, cancellationToken);
        await AddTracksAsync(topTracks, spotifyConfig.PlaylistId, client, cancellationToken);

        await UpdatePlaylistDescriptionAsync(topTracks.First(), client, spotifyConfig.PlaylistId, cancellationToken);

        logger.LogInformation("Playlist update completed at: {Time}", DateTime.Now);
    }

    private async Task UpdatePlaylistDescriptionAsync(FullTrack fullTrack, ISpotifyClient client, string playlistId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating playlist description...");

        string description = GetPlaylistDescription(fullTrack, cancellationToken);

        PlaylistChangeDetailsRequest updatePlaylistDetailsRequest = new()
        {
            Description = description
        };

        _ = await client.Playlists.ChangeDetails(playlistId, updatePlaylistDetailsRequest, cancellationToken);

        logger.LogInformation("Playlist description updated");
    }

    private string GetPlaylistDescription(FullTrack fullTrack, CancellationToken cancellationToken)
    {
        string artistName = fullTrack.Artists.First().Name;
        string updateDateTimeValue = DateTime.UtcNow.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
        return $"{artistName} at the top. Last update: {updateDateTimeValue}";
    }

    private async Task AddTracksAsync(IEnumerable<FullTrack> topTracks, string playlistId, ISpotifyClient client, CancellationToken cancellationToken)
    {
        logger.LogInformation("Adding top tracks to the playlist...");
        
        PlaylistReplaceItemsRequest replaceItemsRequest = new(topTracks.Select(t => t.Uri).ToList());
        _ = await client.Playlists.ReplaceItems(playlistId, replaceItemsRequest, cancellationToken);

        logger.LogInformation("Added {Count} tracks to the playlist.", topTracks.Count());
    }

    private async Task<IEnumerable<FullTrack>> GetTopTracksAsync(ISpotifyClient client, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching top tracks...");

        UsersTopTracksResponse topTracksResponse = await client.UserProfile.GetTopTracks(new UsersTopItemsRequest(TimeRange.ShortTerm) { Limit = 50 }, cancellationToken);
        logger.LogInformation("Fetched {Count} top tracks.", topTracksResponse.Items.Count);

        return topTracksResponse.Items;
    }

    private async Task RemoveCurrentTracksAsync(ISpotifyClient client, string playlistId, CancellationToken cancellationToken)
    {
        IEnumerable<FullTrack>? currentTracks = await GetCurrentPlaylistTracksAsync(client, playlistId, cancellationToken);
        if (currentTracks == null || !currentTracks.Any())
        {
            return;
        }

        logger.LogInformation("Removing current tracks from the playlist...");

        PlaylistRemoveItemsRequest playlistRemoveItemsRequest = new()
        {
            Tracks = currentTracks.Select(ct => new PlaylistRemoveItemsRequest.Item { Uri = ct.Uri }).ToList(),
        };

        _ = await client.Playlists.RemoveItems(playlistId, playlistRemoveItemsRequest, cancellationToken);

        logger.LogInformation("Current tracks removed from the playlist.");
    }

    private async Task<IEnumerable<FullTrack>?> GetCurrentPlaylistTracksAsync(ISpotifyClient client, string playlistId, CancellationToken cancellationToken)
    {
        Paging<PlaylistTrack<IPlayableItem>> playlistItems = await client.Playlists.GetItems(playlistId, new PlaylistGetItemsRequest(PlaylistGetItemsRequest.AdditionalTypes.Track), cancellationToken);
        if (playlistItems.Items == null)
        {
            return null;
        }

        List<FullTrack> tracks = [];
        foreach (PlaylistTrack<IPlayableItem> item in playlistItems.Items)
        {
            if (item.Track is FullTrack track)
            {
                tracks.Add(track);
            }
        }

        return tracks;
    }
}
