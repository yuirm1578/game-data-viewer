using System.Text.Json;
using GameDataViewer.Core.Models;
using Microsoft.Extensions.Logging;

namespace GameDataViewer.Infrastructure.Services;

/// <summary>AppSettings를 %LOCALAPPDATA%\GameDataViewer\settings.json에 저장/로드</summary>
public class AppSettingsService
{
    private readonly ILogger<AppSettingsService> _logger;
    private readonly string _settingsPath;
    private AppSettings _current;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public AppSettingsService(ILogger<AppSettingsService> logger)
    {
        _logger = logger;
        _settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GameDataViewer", "settings.json");
        _current = Load();
    }

    public AppSettings Current => _current;

    public AppSettings Load()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                _current = JsonSerializer.Deserialize<AppSettings>(json, JsonOpts) ?? new AppSettings();
                _logger.LogDebug("Settings loaded from {Path}", _settingsPath);
                return _current;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load settings. Using defaults.");
        }
        _current = new AppSettings();
        return _current;
    }

    public void Save(AppSettings? settings = null)
    {
        if (settings != null) _current = settings;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
            File.WriteAllText(_settingsPath, JsonSerializer.Serialize(_current, JsonOpts));
            _logger.LogDebug("Settings saved to {Path}", _settingsPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings.");
        }
    }
}
