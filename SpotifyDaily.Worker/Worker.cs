using SpotifyDaily.Worker.Exceptions;
using SpotifyDaily.Worker.Helpers;
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

            if (_appConfig == null)
            {
                throw new WorkerException("AppConfig cannot be retrieved.");
            }

            logger.LogInformation("Spotify Daily Worker started at: {Time}", DateTime.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                DateTime now = DateTime.Now;
                DateOnly dateNow = DateOnly.FromDateTime(DateTime.Now);
                DateOnly lastRun = DateOnly.FromDateTime(_appConfig.LastRun ?? DateTime.MinValue);

                //If the worker doesn't run today, run it
                if (lastRun >= dateNow)
                {
                    await WaitForNextRun(now, stoppingToken);
                    continue;
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
            var nextRunDelay = date.CalculateNextRunDelay();
            logger.LogInformation("Waiting for the next run at: {Time}", date.Add(nextRunDelay));
            await Task.Delay(nextRunDelay, cancellationToken);
        }

        private void OnAppConfigChanged(AppConfig config)
        {
            _appConfig = config;
        }
    }
}
