using AutoFixture;
using AutoFixture.Xunit2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SpotifyAPI.Web;
using SpotifyDaily.Tests.Worker.Attributes;
using SpotifyDaily.Worker.Exceptions;
using SpotifyDaily.Worker.Options;
using SpotifyDaily.Worker.Services;
using SpotifyDaily.Worker.Services.Contracts;

namespace SpotifyDaily.Tests.Worker.Services;

public class PlaylistServiceTests
{
    private SpotifyOptions CreateSpotifyOptions(string? playlistId = "playlist123")
    {
        return new SpotifyOptions
        {
            ClientId = "clientId",
            ClientSecret = "clientSecret",
            PlaylistId = playlistId,
            RedirectURI = "http://localhost/callback"
        };
    }

    [Theory]
    [AutoMoqData]
    public async Task UpdateDailyPlaylistAsync_ShouldThrowException_WhenSpotifyClientIsNotConfigured(
        [Frozen] Mock<ISpotifyClientService> spotifyClientServiceMock,
        [Frozen] Mock<IOptions<SpotifyOptions>> spotifyOptionsMock,
        [Frozen] Mock<ILogger<PlaylistService>> loggerMock)
    {
        // Arrange
        _ = spotifyClientServiceMock.Setup(x => x.GetClientAsync(null, It.IsAny<CancellationToken>()))
                                     .ThrowsAsync(new PlaylistServiceException("Spotify client is not configured. Please call ConfigureClientAsync first."));

        _ = spotifyOptionsMock.Setup(x => x.Value).Returns(CreateSpotifyOptions());

        PlaylistService service = new(
            loggerMock.Object,
            spotifyOptionsMock.Object,
            spotifyClientServiceMock.Object);

        // Act & Assert
        _ = await Assert.ThrowsAsync<PlaylistServiceException>(
            () => service.UpdateDailyPlaylistAsync());
    }

    [Theory]
    [AutoMoqData]
    public async Task UpdateDailyPlaylistAsync_ShouldThrowException_WhenPlaylistIdIsMissing(
        [Frozen] Mock<SpotifyClient> spotifyClientMock,
        [Frozen] Mock<ISpotifyClientService> spotifyClientServiceMock,
        [Frozen] Mock<ILogger<PlaylistService>> loggerMock,
        [Frozen] Mock<IOptions<SpotifyOptions>> spotifyOptionsMock)
    {
        // Arrange
        _ = spotifyClientServiceMock.Setup(x => x.GetClientAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(spotifyClientMock.Object);

        _ = spotifyOptionsMock.Setup(x => x.Value).Returns(CreateSpotifyOptions(playlistId: null));

        PlaylistService service = new(
            loggerMock.Object,
            spotifyOptionsMock.Object,
            spotifyClientServiceMock.Object);

        // Act & Assert
        _ = await Assert.ThrowsAsync<PlaylistServiceException>(
            () => service.UpdateDailyPlaylistAsync());
    }

    [Theory]
    [AutoMoqData]
    public async Task UpdateDailyPlaylistAsync_ShouldCallAllMethods_WhenConfigurationIsValid(
        Fixture fixture,
        [Frozen] Mock<ISpotifyClientService> spotifyClientServiceMock,
        [Frozen] Mock<IPlaylistsClient> playlistsClientMock,
        [Frozen] Mock<IUserProfileClient> userProfileClientMock,
        [Frozen] Mock<IOptions<SpotifyOptions>> spotifyOptionsMock,
        [Frozen] Mock<ILogger<PlaylistService>> loggerMock)
    {
        // Arrange
        string playlistId = "playlist123";
        FullTrack fullTrack = fixture.Freeze<FullTrack>();
        List<FullTrack> topTracks = new()
        { fullTrack };
        List<FullTrack> playlistTracks = new()
        { fullTrack };

        // Create the fake client with the mocked interfaces
        FakeSpotifyClient fakeClient = new(
            playlistsClientMock.Object,
            userProfileClientMock.Object);

        // Setup client service to return our fake client
        _ = spotifyClientServiceMock.Setup(x => x.GetClientAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeClient);

        _ = spotifyOptionsMock.Setup(x => x.Value).Returns(CreateSpotifyOptions(playlistId));

        _ = playlistsClientMock.Setup(x => x.GetItems(playlistId, It.IsAny<PlaylistGetItemsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Paging<PlaylistTrack<IPlayableItem>>
            {
                Items = playlistTracks.Select(t => new PlaylistTrack<IPlayableItem> { Track = t }).ToList()
            });

        _ = playlistsClientMock.Setup(x => x.RemoveItems(playlistId, It.IsAny<PlaylistRemoveItemsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SnapshotResponse());

        _ = userProfileClientMock.Setup(x => x.GetTopTracks(It.IsAny<UsersTopItemsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UsersTopTracksResponse
            {
                Items = topTracks
            });

        _ = playlistsClientMock.Setup(x => x.ReplaceItems(playlistId, It.IsAny<PlaylistReplaceItemsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SnapshotResponse());

        _ = playlistsClientMock.Setup(x => x.ChangeDetails(playlistId, It.IsAny<PlaylistChangeDetailsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        PlaylistService service = new(
            loggerMock.Object,
            spotifyOptionsMock.Object,
            spotifyClientServiceMock.Object);

        // Act
        await service.UpdateDailyPlaylistAsync();

        // Assert
        playlistsClientMock.Verify(x => x.GetItems(playlistId, It.IsAny<PlaylistGetItemsRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        playlistsClientMock.Verify(x => x.RemoveItems(playlistId, It.IsAny<PlaylistRemoveItemsRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        userProfileClientMock.Verify(x => x.GetTopTracks(It.IsAny<UsersTopItemsRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        playlistsClientMock.Verify(x => x.ReplaceItems(playlistId, It.IsAny<PlaylistReplaceItemsRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        playlistsClientMock.Verify(x => x.ChangeDetails(playlistId, It.IsAny<PlaylistChangeDetailsRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}