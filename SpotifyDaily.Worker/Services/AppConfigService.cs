using Microsoft.Extensions.Options;
using SpotifyDaily.Worker.Models;
using SpotifyDaily.Worker.Services.Contracts;
using System.Text.Json;

namespace SpotifyDaily.Worker.Services;

public class AppConfigService : IAppConfigService, IDisposable
{
    private readonly string _file;
    private readonly FileSystemWatcher _watcher;
    private readonly SemaphoreSlim _writeLock = new(1);

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    private AppConfig _appConfig;

    public AppConfigService(
      IHostEnvironment env,
      IConfiguration cfg,
      IOptionsMonitor<AppConfig> options)
    {
        _appConfig = options.CurrentValue;
        // figure out where the JSON lives
        _file = Path.Combine(env.ContentRootPath, "appconfig.json");

        // watch the file for external edits
        _watcher = new FileSystemWatcher(Path.GetDirectoryName(_file)!, Path.GetFileName(_file))
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
        };
        _watcher.Changed += (s, e) =>
        {
            // delay to ensure file is done writing
            _ = Task.Delay(200).ContinueWith(_ =>
            {
                OnChange?.Invoke(Current);
            });
        };
        _watcher.EnableRaisingEvents = true;
    }

    public AppConfig Current
    {
        get
        {
            return _appConfig;
        }
        set
        {
            UpdateAsync(value).GetAwaiter().GetResult();
            _appConfig = value;
            OnChange?.Invoke(value);
        }
    }

    public async Task UpdateAsync(AppConfig newSettings)
    {
        await _writeLock.WaitAsync();
        try
        {
            string json = JsonSerializer.Serialize(new { AppConfig = newSettings}, _jsonOptions);
            // atomically replace the file
            File.WriteAllText(_file, json);
            // force a reload so IOptionsMonitor fires immediately
            OnChange?.Invoke(Current);
        }
        finally
        {
            _ = _writeLock.Release();
        }
    }

    public event Action<AppConfig>? OnChange;

    public void Dispose()
    {
        _watcher.Dispose();
        _writeLock.Dispose();
    }
}