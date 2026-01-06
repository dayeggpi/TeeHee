using System.IO;
using System.Text.Json;

namespace TeeHee;

public class TriggerDatabase
{
    private static readonly Lazy<TriggerDatabase> _instance = new(() => new TriggerDatabase());
    public static TriggerDatabase Instance => _instance.Value;

    private static string? _customDatabasePath;
    
    public static string DatabasePath
    {
        get
        {
            if (!string.IsNullOrEmpty(_customDatabasePath))
                return _customDatabasePath;
            
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "TeeHee",
                "triggers.json");
        }
    }

    public static string SettingsPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TeeHee",
        "settings.json");

    public List<Trigger> Triggers { get; set; } = new();
    public AppSettings Settings { get; set; } = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private TriggerDatabase() { }

    public void Load()
    {
        // Load settings first (contains custom path)
        LoadSettings();
        
        // Then load triggers from configured path
        LoadTriggers();
    }

    private void LoadSettings()
    {
        try
        {
            var dir = Path.GetDirectoryName(SettingsPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                if (settings != null)
                {
                    Settings = settings;
                    _customDatabasePath = settings.CustomDatabasePath;
                }
            }
        }
        catch
        {
            Settings = new AppSettings();
        }
    }

    private void LoadTriggers()
    {
        try
        {
            if (File.Exists(DatabasePath))
            {
                var json = File.ReadAllText(DatabasePath);
                var data = JsonSerializer.Deserialize<DatabaseData>(json);
                if (data != null)
                {
                    Triggers = data.Triggers ?? new List<Trigger>();
                }
            }
            else
            {
                Triggers.Add(new Trigger { Input = ":hi", Output = "hello world" });
                SaveTriggers();
            }
        }
        catch
        {
            Triggers = new List<Trigger> { new() { Input = ":hi", Output = "hello world" } };
        }
    }

    public void Save()
    {
        SaveSettings();
        SaveTriggers();
    }

    private void SaveSettings()
    {
        try
        {
            var dir = Path.GetDirectoryName(SettingsPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(Settings, JsonOptions);
            File.WriteAllText(SettingsPath, json);
        }
        catch { }
    }

    private void SaveTriggers()
    {
        try
        {
            var dir = Path.GetDirectoryName(DatabasePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var data = new DatabaseData { Triggers = Triggers };
            var json = JsonSerializer.Serialize(data, JsonOptions);
            File.WriteAllText(DatabasePath, json);
        }
        catch { }
    }

    public void SetCustomDatabasePath(string? path)
    {
        _customDatabasePath = path;
        Settings.CustomDatabasePath = path;
        SaveSettings();
    }

    public void Export(string path)
    {
        var data = new DatabaseData { Triggers = Triggers };
        var json = JsonSerializer.Serialize(data, JsonOptions);
        File.WriteAllText(path, json);
    }

    public void Import(string path, bool merge = true)
    {
        var json = File.ReadAllText(path);
        
        // Strict validation
        DatabaseData? data;
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = false // Strict property names
            };
            data = JsonSerializer.Deserialize<DatabaseData>(json, options);
        }
        catch (JsonException ex)
        {
            throw new Exception($"Invalid JSON format: {ex.Message}");
        }

        if (data == null)
        {
            throw new Exception("Failed to parse JSON: empty or invalid structure");
        }

        if (data.Triggers == null)
        {
            throw new Exception("Invalid format: missing 'Triggers' array");
        }

        // Validate each trigger has required fields
        foreach (var trigger in data.Triggers)
        {
            if (string.IsNullOrEmpty(trigger.Input))
            {
                throw new Exception("Invalid format: trigger missing 'Input' field");
            }
            if (trigger.Output == null)
            {
                throw new Exception("Invalid format: trigger missing 'Output' field");
            }
        }

        if (merge)
        {
            foreach (var trigger in data.Triggers)
            {
                if (!Triggers.Any(t => t.Input == trigger.Input))
                {
                    Triggers.Add(trigger);
                }
            }
        }
        else
        {
            Triggers = data.Triggers;
        }

        SaveTriggers();
    }

    public Dictionary<string, string> GetTriggerDictionary()
    {
        return Triggers.ToDictionary(t => t.Input, t => t.Output);
    }
}

internal class DatabaseData
{
    public List<Trigger>? Triggers { get; set; }
}