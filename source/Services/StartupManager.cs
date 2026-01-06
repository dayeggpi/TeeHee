using Microsoft.Win32;
using System.Reflection;
using System.Diagnostics;
namespace TeeHee;

public static class StartupManager
{
    private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "TeeHee";

    public static bool IsRegisteredForStartup()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
            return key?.GetValue(AppName) != null;
        }
        catch
        {
            return false;
        }
    }

    public static void RegisterForStartup()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
            var exePath = GetExecutablePath();
            key?.SetValue(AppName, $"\"{exePath}\"");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to register for startup: {ex.Message}");
        }
    }

    public static void UnregisterFromStartup()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
            key?.DeleteValue(AppName, false);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to unregister from startup: {ex.Message}");
        }
    }

    private static string GetExecutablePath()
    {
        var location = Process.GetCurrentProcess().MainModule?.FileName;
        
        // For single-file deployments, use Environment.ProcessPath
        if (string.IsNullOrEmpty(location) || location.EndsWith(".dll"))
        {
            return Environment.ProcessPath ?? string.Empty;
        }
        
        return location;
    }
}
