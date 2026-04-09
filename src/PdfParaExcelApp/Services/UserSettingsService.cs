using System.Text.Json;
using System.IO;

namespace PdfParaExcelApp.Services;

public class UserSettingsService : IUserSettingsService
{
    private readonly string _settingsPath;
    private SettingsModel _settings;

    public UserSettingsService()
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PdfParaExcelApp");
        Directory.CreateDirectory(folder);
        _settingsPath = Path.Combine(folder, "settings.json");
        _settings = Load();
    }

    public string? LastPdfPath
    {
        get => _settings.LastPdfPath;
        set => _settings.LastPdfPath = value;
    }

    public string? LastOutputPath
    {
        get => _settings.LastOutputPath;
        set => _settings.LastOutputPath = value;
    }

    public void Save()
    {
        File.WriteAllText(_settingsPath, JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true }));
    }

    private SettingsModel Load()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                return new SettingsModel();
            }

            return JsonSerializer.Deserialize<SettingsModel>(File.ReadAllText(_settingsPath)) ?? new SettingsModel();
        }
        catch
        {
            return new SettingsModel();
        }
    }

    private sealed class SettingsModel
    {
        public string? LastPdfPath { get; set; }
        public string? LastOutputPath { get; set; }
    }
}
