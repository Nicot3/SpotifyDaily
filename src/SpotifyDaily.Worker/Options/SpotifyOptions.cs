namespace SpotifyDaily.Worker.Options;

public class SpotifyOptions
{
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? PlaylistId { get; set; }
    public string RedirectURI { get; set; } = string.Empty;
}
