using SpotifyDaily.Worker.Models;
using SpotifyDaily.Worker.Services.Contracts;

namespace SpotifyDaily.Worker
{
    public class Worker(ILogger<Worker> logger, IAppConfigService appConfigService, IPlaylistService playlistService) : BackgroundService
    {
        private AppConfig? _appConfig;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            appConfigService.OnChange += OnAppConfigChanged;

            _appConfig = appConfigService.Current;

            while (!stoppingToken.IsCancellationRequested)
            {
                DateTime now = DateTime.Now;
                DateTime? lastRun = _appConfig.LastRun;

                //If the worker doesn't run today, run it
                if (lastRun != null && !(lastRun?.Date < now.Date))
                {
                    await WaitForNextRun(now, stoppingToken);
                }
                try
                {
                    logger.LogInformation("Running Spotify Daily Worker at: {Time}", now);

                    await playlistService.UpdateDailyPlaylistAsync(stoppingToken);

                    _appConfig.LastRun = now;
                    await appConfigService.UpdateAsync(_appConfig);

                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while running the Spotify Daily Worker.");
                }
                finally
                {
                    await WaitForNextRun(now, stoppingToken);
                }
            }
        }

        private async Task WaitForNextRun(DateTime date, CancellationToken cancellationToken)
        {
                logger.LogInformation("Waiting for the next run at: {Time}", date.AddMinutes(1));
            // Wait for the next run if the playlist service is not configured
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        }

        private void OnAppConfigChanged(AppConfig config)
        {
            _appConfig = config;
        }
    }
}
