namespace SpotifyDaily.Worker.Models;

public class AppConfig
{
    public DateTime? LastRun { get; set; } = DateTime.MinValue;
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpireDate { get; set; }
}
