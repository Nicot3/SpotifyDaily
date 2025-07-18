using SpotifyDaily.Worker.Models;
using SpotifyDaily.Worker.Options;
using SpotifyDaily.Worker.Services;
using SpotifyDaily.Worker.Services.Contracts;

namespace SpotifyDaily.Worker;

public static class Bootstrapper
{
    public static IHostApplicationBuilder ConfigureSpotifyDaily(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IAppConfigService, AppConfigService>();
        builder.Services.AddSingleton<ISpotifyClientService, SpotifyClientService>();
        builder.Services.AddSingleton<IPlaylistService, PlaylistService>();


        builder.Configuration.AddJsonFile("appconfig.json", optional: false, reloadOnChange: true);

        builder.Services.Configure<AppConfig>(builder.Configuration.GetSection("AppConfig"));
        builder.Services.Configure<SpotifyOptions>(builder.Configuration.GetSection("Spotify"));
        builder.Services.Configure<WorkerOptions>(builder.Configuration.GetSection("Worker"));

        return builder;
    }
}
