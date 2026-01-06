namespace TeeHee;

public class AppSettings
{
    public int TriggerSpeed { get; set; } = 5;
    public string? CustomDatabasePath { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string? Theme { get; set; } = "Light";  // Add this line

    public int GetDelayMs() => Math.Max(3, 33 - (TriggerSpeed * 3));
}