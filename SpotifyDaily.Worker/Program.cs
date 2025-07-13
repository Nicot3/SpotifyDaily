using SpotifyDaily.Worker.Models;
using SpotifyDaily.Worker.Options;
using SpotifyDaily.Worker.Services;
using SpotifyDaily.Worker.Services.Contracts;

namespace SpotifyDaily.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRouting();
            builder.Services.AddControllers();
            builder.Services.AddHostedService<Worker>();

            builder.Services.AddSingleton<IAppConfigService, AppConfigService>();
            builder.Services.AddSingleton<ISpotifyClientService, SpotifyClientService>();
            builder.Services.AddSingleton<IPlaylistService, PlaylistService>();

            builder.Configuration.AddJsonFile("appconfig.json", optional: false, reloadOnChange: true);
            builder.Services.Configure<AppConfig>(builder.Configuration.GetSection("AppConfig"));

            builder.Services.Configure<SpotifyOptions>(builder.Configuration.GetSection("Spotify"));

            var app = builder.Build();

            app.MapControllers();

            app.Run();
        }
    }
}