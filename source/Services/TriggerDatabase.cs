using System.IO;
using System.Text.Json;
using System.Reflection;

namespace TeeHee;

public class TriggerDatabase
{
    private static readonly Lazy<TriggerDatabase> _instance = new(() => new TriggerDatabase());
    public static TriggerDatabase Instance => _instance.Value;

    private static string? _customDatabasePath;
    private static string? _settingsFilePath;
    
    // Get the folder where the executable is located
    public static string PortableFolderPath => 
        Path.GetDirectoryName(Environment.ProcessPath) ?? AppDomain.CurrentDomain.BaseDirectory;
    
    public static string PortableSettingsPath => 
        Path.Combine(PortableFolderPath, "settings.json");
    
    public static string AppDataFolderPath => 
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TeeHee");
    
    public static string AppDataSettingsPath => 
        Path.Combine(AppDataFolderPath, "settings.json");

    public static string DatabasePath
    {
        get
        {
            if (!string.IsNullOrEmpty(_customDatabasePath))
                return _customDatabasePath;
            
            // Use same folder as settings file
            if (!string.IsNullOrEmpty(_settingsFilePath))
            {
                var dir = Path.GetDirectoryName(_settingsFilePath);
                if (!string.IsNullOrEmpty(dir))
                    return Path.Combine(dir, "triggers.json");
            }
            
            return Path.Combine(AppDataFolderPath, "triggers.json");
        }
    }

    public static string SettingsPath => _settingsFilePath ?? AppDataSettingsPath;

    public List<Trigger> Triggers { get; set; } = new();
    public AppSettings Settings { get; set; } = new();
    
    // Returns true if settings were found, false if user needs to choose location
    public bool SettingsFound { get; private set; }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private TriggerDatabase() { }

    // Call this first to detect settings location
    public SettingsLocationResult DetectSettingsLocation()
    {
        // Priority 1: Check portable location (same folder as exe)
        if (File.Exists(PortableSettingsPath))
        {
            _settingsFilePath = PortableSettingsPath;
            SettingsFound = true;
            return SettingsLocationResult.FoundPortable;
        }
        
        // Priority 2: Check AppData location
        if (File.Exists(AppDataSettingsPath))
        {
            _settingsFilePath = AppDataSettingsPath;
            SettingsFound = true;
            return SettingsLocationResult.FoundAppData;
        }
        
        // Not found in either location
        SettingsFound = false;
        return SettingsLocationResult.NotFound;
    }

	public void SetSettingsLocation(SettingsLocationMode mode, string? customPath = null)
	{
		switch (mode)
		{
			case SettingsLocationMode.Portable:
				_settingsFilePath = PortableSettingsPath;
				_customDatabasePath = null; // Use default (same folder as settings)
				break;
			case SettingsLocationMode.AppData:
				_settingsFilePath = AppDataSettingsPath;
				_customDatabasePath = null; // Use default (same folder as settings)
				break;
			case SettingsLocationMode.Custom:
				// Custom mode: settings go portable, only triggers go to custom path
				_settingsFilePath = PortableSettingsPath;
				if (!string.IsNullOrEmpty(customPath))
				{
					_customDatabasePath = customPath.EndsWith("triggers.json") 
						? customPath 
						: Path.Combine(customPath, "triggers.json");
				}
				break;
		}
		SettingsFound = true;
	}

	public SettingsLocationMode GetCurrentLocationMode()
	{
		// If there's a custom database path, it's Custom mode
		if (!string.IsNullOrEmpty(_customDatabasePath))
			return SettingsLocationMode.Custom;
		
		if (string.IsNullOrEmpty(_settingsFilePath))
			return SettingsLocationMode.AppData;
			
		if (_settingsFilePath.Equals(PortableSettingsPath, StringComparison.OrdinalIgnoreCase))
			return SettingsLocationMode.Portable;
			
		if (_settingsFilePath.Equals(AppDataSettingsPath, StringComparison.OrdinalIgnoreCase))
			return SettingsLocationMode.AppData;
			
		return SettingsLocationMode.Portable;
	}

	public void MoveSettingsTo(SettingsLocationMode newMode, string? customPath = null)
	{
		var oldSettingsPath = SettingsPath;
		var oldTriggersPath = DatabasePath;
		
		SetSettingsLocation(newMode, customPath);
		
		// Update the custom path in settings before saving
		Settings.CustomDatabasePath = _customDatabasePath;
		
		var newSettingsPath = SettingsPath;
		var newTriggersPath = DatabasePath;
		
		// Save to new location
		Save();
		
		// Delete old files if they differ from new locations
		try
		{
			if (File.Exists(oldSettingsPath) && !oldSettingsPath.Equals(newSettingsPath, StringComparison.OrdinalIgnoreCase))
				File.Delete(oldSettingsPath);
			if (File.Exists(oldTriggersPath) && !oldTriggersPath.Equals(newTriggersPath, StringComparison.OrdinalIgnoreCase))
				File.Delete(oldTriggersPath);
		}
		catch { }
	}

    public void Load()
    {
        LoadSettings();
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
        
        DatabaseData? data;
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = false
            };
            data = JsonSerializer.Deserialize<DatabaseData>(json, options);
        }
        catch (JsonException ex)
        {
            throw new Exception($"Invalid JSON format: {ex.Message}");
        }

        if (data == null)
            throw new Exception("Failed to parse JSON: empty or invalid structure");

        if (data.Triggers == null)
            throw new Exception("Invalid format: missing 'Triggers' array");

        foreach (var trigger in data.Triggers)
        {
            if (string.IsNullOrEmpty(trigger.Input))
                throw new Exception("Invalid format: trigger missing 'Input' field");
            if (trigger.Output == null)
                throw new Exception("Invalid format: trigger missing 'Output' field");
            if (trigger.Input.Length > 62)
                throw new Exception($"Invalid format: trigger input '{trigger.Input}' exceeds 62 characters");
        }

        if (merge)
        {
            foreach (var trigger in data.Triggers)
            {
                if (!Triggers.Any(t => t.Input == trigger.Input))
                    Triggers.Add(trigger);
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

public enum SettingsLocationResult
{
    FoundPortable,
    FoundAppData,
    NotFound
}

internal class DatabaseData
{
    public List<Trigger>? Triggers { get; set; }
}