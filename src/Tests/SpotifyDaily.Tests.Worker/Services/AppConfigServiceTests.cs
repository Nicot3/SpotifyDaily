using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using SpotifyDaily.Worker.Models;
using SpotifyDaily.Worker.Services;
using System.Text.Json;

namespace SpotifyDaily.Tests.Worker.Services;

public class AppConfigServiceTests : IDisposable
{
    private readonly Mock<IHostEnvironment> _mockEnvironment;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IOptionsMonitor<AppConfig>> _mockOptionsMonitor;
    private readonly string _tempDir;
    private readonly string _tempFile;
    private readonly AppConfig _initialConfig;

    public AppConfigServiceTests()
    {
        _initialConfig = new AppConfig 
        { 
            LastRun = DateTime.Now.AddDays(-1),
            Token = "test-token",
            RefreshToken = "test-refresh-token",
            ExpireDate = DateTime.Now.AddDays(1)
        };

        _mockEnvironment = new Mock<IHostEnvironment>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockOptionsMonitor = new Mock<IOptionsMonitor<AppConfig>>();
        _mockOptionsMonitor.Setup(m => m.CurrentValue).Returns(_initialConfig);

        // Setup temp directory for tests
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
        _tempFile = Path.Combine(_tempDir, "appconfig.json");
        
        // Setup mock environment to return our temp directory
        _mockEnvironment.Setup(e => e.ContentRootPath).Returns(_tempDir);
    }

    [Fact]
    public void Constructor_InitializesProperties()
    {
        // Arrange & Act
        var service = CreateService();

        // Assert
        Assert.Equal(_initialConfig, service.Current);
    }

    [Fact]
    public void Current_Get_ReturnsCurrentConfig()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = service.Current;

        // Assert
        Assert.Equal(_initialConfig, result);
    }

    [Fact]
    public void Current_Set_UpdatesConfigAndRaisesEvent()
    {
        // Arrange
        var service = CreateService();
        var newConfig = new AppConfig { Token = "new-token" };
        var eventRaised = false;
        service.OnChange += config => eventRaised = config.Token == "new-token";

        // Act
        service.Current = newConfig;

        // Assert
        Assert.True(eventRaised, "OnChange event should be raised");
        Assert.Equal("new-token", service.Current.Token);
        Assert.True(File.Exists(_tempFile), "Config file should be created");

        // Verify file contents
        var fileContent = File.ReadAllText(_tempFile);
        using var doc = JsonDocument.Parse(fileContent);
        var appConfigJson = doc.RootElement.GetProperty("AppConfig").GetRawText();
        var deserializedContent = JsonSerializer.Deserialize<AppConfig>(appConfigJson);
        Assert.NotNull(deserializedContent);
        Assert.Equal("new-token", deserializedContent!.Token);
    }

    [Fact]
    public async Task UpdateAsync_WritesConfigToFile()
    {
        // Arrange
        var service = CreateService();
        var newConfig = new AppConfig 
        { 
            LastRun = DateTime.Now,
            Token = "updated-token",
            RefreshToken = "updated-refresh"
        };
        
        // Act
        await service.UpdateAsync(newConfig);

        // Assert
        Assert.True(File.Exists(_tempFile), "Config file should be created");
        
        // Verify file contents
        var fileContent = File.ReadAllText(_tempFile);
        using var doc = JsonDocument.Parse(fileContent);
        var appConfigJson = doc.RootElement.GetProperty("AppConfig").GetRawText();
        var deserializedContent = JsonSerializer.Deserialize<AppConfig>(appConfigJson);
        Assert.NotNull(deserializedContent);
        Assert.Equal("updated-token", deserializedContent!.Token);
    }

    private AppConfigService CreateService()
    {
        return new AppConfigService(
            _mockEnvironment.Object,
            _mockConfiguration.Object,
            _mockOptionsMonitor.Object);
    }

    public void Dispose()
    {
        // Clean up temp files
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }
        catch { /* Ignore cleanup errors */ }
    }
}
