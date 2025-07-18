using SpotifyAPI.Web;
using SpotifyAPI.Web.Http;

public class FakeSpotifyClient : ISpotifyClient
{
    public IPlaylistsClient Playlists { get; } = default!;
    public IUserProfileClient UserProfile { get; } = default!;

    // Add any other required properties from ISpotifyClient interface with default implementations

    public FakeSpotifyClient()
    {
        
    }

    public FakeSpotifyClient(IPlaylistsClient playlists, IUserProfileClient userProfile)
    {
        Playlists = playlists;
        UserProfile = userProfile;
    }


    public AlbumsClient Albums => throw new NotImplementedException();
    public ArtistsClient Artists => throw new NotImplementedException();
    public BrowseClient Browse => throw new NotImplementedException();

    public IPaginator DefaultPaginator => throw new NotImplementedException();

    IUserProfileClient ISpotifyClient.UserProfile => UserProfile;

    IBrowseClient ISpotifyClient.Browse => Browse;

    public IShowsClient Shows => throw new NotImplementedException();

    IPlaylistsClient ISpotifyClient.Playlists => Playlists;

    public ISearchClient Search => throw new NotImplementedException();

    public IFollowClient Follow => throw new NotImplementedException();

    public ITracksClient Tracks => throw new NotImplementedException();

    public IPlayerClient Player => throw new NotImplementedException();

    IAlbumsClient ISpotifyClient.Albums => Albums;

    IArtistsClient ISpotifyClient.Artists => Artists;

    public IPersonalizationClient Personalization => throw new NotImplementedException();

    public IEpisodesClient Episodes => throw new NotImplementedException();

    public ILibraryClient Library => throw new NotImplementedException();

    public IAudiobooksClient Audiobooks => throw new NotImplementedException();

    public IChaptersClient Chapters => throw new NotImplementedException();

    public IResponse? LastResponse => throw new NotImplementedException();

    // Add other required properties with placeholder implementations

    public void Dispose() { }

    public Task<IList<T>> PaginateAll<T>(IPaginatable<T> firstPage, IPaginator? paginator = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IList<T>> PaginateAll<T, TNext>(IPaginatable<T, TNext> firstPage, Func<TNext, IPaginatable<T, TNext>> mapper, IPaginator? paginator = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<T> Paginate<T>(IPaginatable<T> firstPage, IPaginator? paginator = null, CancellationToken cancel = default)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<T> Paginate<T, TNext>(IPaginatable<T, TNext> firstPage, Func<TNext, IPaginatable<T, TNext>> mapper, IPaginator? paginator = null, CancellationToken cancel = default)
    {
        throw new NotImplementedException();
    }

    public Task<Paging<T>> NextPage<T>(Paging<T> paging)
    {
        throw new NotImplementedException();
    }

    public Task<CursorPaging<T>> NextPage<T>(CursorPaging<T> cursorPaging)
    {
        throw new NotImplementedException();
    }

    public Task<TNext> NextPage<T, TNext>(IPaginatable<T, TNext> paginatable)
    {
        throw new NotImplementedException();
    }

    public Task<Paging<T>> PreviousPage<T>(Paging<T> paging)
    {
        throw new NotImplementedException();
    }

    public Task<TNext> PreviousPage<T, TNext>(Paging<T, TNext> paging)
    {
        throw new NotImplementedException();
    }
}